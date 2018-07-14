using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Activout.RestClient.Helpers.Implementation
{
    /*
     * Convert from Task<object> to Task<T> where T is the Type
     * 
     * Implemented by executing Task<object>.ContinueWith<T>(x => (T)x.Result) using reflection.
     */
    class TaskConverter : ITaskConverter
    {
        private static readonly Type objectTaskType = typeof(Task<object>);
        private readonly MethodInfo continueWith;
        private readonly Delegate lambda;

        public TaskConverter(Type actualReturnType)
        {
            lambda = CreateLambda(actualReturnType);
            continueWith = GetContinueWithMethod(actualReturnType);
        }

        public object ConvertReturnType(Task<object> task)
        {
            return continueWith.Invoke(task, new object[] { lambda });
        }

        private static MethodInfo GetContinueWithMethod(Type actualReturnType)
        {
            // Inspired by https://stackoverflow.com/a/3632196/20444
            var baseFuncType = typeof(Func<,>);
            var continueWithMethod = objectTaskType.GetMethods()
                .Where(x => x.Name == nameof(Task.ContinueWith) && x.GetParameters().Length == 1)
                .Select(x => new { M = x, P = x.GetParameters() })
                .Where(x => x.P[0].ParameterType.IsGenericType && x.P[0].ParameterType.GetGenericTypeDefinition() == baseFuncType)
                .Select(x => new { x.M, A = x.P[0].ParameterType.GetGenericArguments() })
                 .Where(x => x.A[0].IsGenericType
                         && x.A[0].GetGenericTypeDefinition() == typeof(Task<>))
                .Select(x => x.M)
                .SingleOrDefault();
            return continueWithMethod.MakeGenericMethod(new Type[] { actualReturnType });
        }

        private static Delegate CreateLambda(Type actualReturnType)
        {
            var constantExpression = Expression.Parameter(objectTaskType);
            var propertyExpression = Expression.Property(constantExpression, "Result");
            var conversion = Expression.Convert(propertyExpression, actualReturnType);

            var lambdaMethod = GetLambdaMethod() ?? throw new NullReferenceException("Failed to get Expression.Lambda method");
            var lambda = InvokeLambdaMethod(lambdaMethod, conversion, constantExpression);
            return lambda.Compile();
        }

        private static LambdaExpression InvokeLambdaMethod(MethodInfo lambdaMethod, UnaryExpression expression, ParameterExpression parameter)
        {
            return (LambdaExpression)lambdaMethod.Invoke(null, new object[] { expression, new ParameterExpression[] { parameter } });
        }

        private static MethodInfo GetLambdaMethod()
        {
            return typeof(Expression)
                .GetMethods()
                .Single(x => x.Name == nameof(Expression.Lambda)
                    && !x.IsGenericMethod
                    && x.GetParameters().Length == 2
                    && x.GetParameters()[0].ParameterType == typeof(Expression)
                    && x.GetParameters()[1].ParameterType == typeof(ParameterExpression[]));
        }
    }
}
