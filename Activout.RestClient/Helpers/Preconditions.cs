using System;

namespace Activout.RestClient.Helpers
{
    // Based on https://gist.github.com/johnlcox/5032524
    public static class Preconditions
    {
        /// <summary>
        /// Ensures that an object reference passed as a parameter to the calling method is not null.
        /// </summary>
        /// <param name="reference">an object reference</param>
        /// <param name="errorMessage">the exception message to use if the check fails</param>
        /// <returns>the non-null reference that was validated</returns>
        /// <exception cref="NullReferenceException">if reference is null</exception>
        public static T CheckNotNull<T>(T reference, string errorMessage = null)
        {
            if (reference == null)
            {
                throw new NullReferenceException(errorMessage);
            }

            return reference;
        }
    }
}
