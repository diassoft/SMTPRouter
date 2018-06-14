using System;
using System.Collections.Generic;
using System.Text;

namespace SMTPRouter
{
    /// <summary>
    /// A class that represents a set of Static functions to be used among the entire application
    /// </summary>
    public static class Utils
    {
        #region Special Character Treatment

        /// <summary>
        /// Lookup array for special characters
        /// </summary>
        /// <remarks>
        /// Valid characters are set to true</remarks>
        private static bool[] _lookup;

        /// <summary>
        /// Internal variable to define whether the lookup array was defined or not
        /// </summary>
        private static bool IsLookupDefined;

        /// <summary>
        /// The lookup array to control which characters are considered special
        /// </summary>
        public static bool[] Lookup
        {
            get
            {
                if (!IsLookupDefined)
                {
                    // Create Lookup Array (valid characters for email address)
                    _lookup = new bool[65536];
                    for (char c = '0'; c <= '9'; c++) _lookup[c] = true;
                    for (char c = 'A'; c <= 'Z'; c++) _lookup[c] = true;
                    for (char c = 'a'; c <= 'z'; c++) _lookup[c] = true;
                    _lookup['!'] = true;
                    _lookup['@'] = true;
                    _lookup['#'] = true;
                    _lookup['$'] = true;
                    _lookup['%'] = true;
                    _lookup['^'] = true;
                    _lookup['&'] = true;
                    _lookup['*'] = true;
                    _lookup['.'] = true;
                    _lookup['-'] = true;
                    _lookup['_'] = true;
                    _lookup['+'] = true;
                    _lookup['='] = true;
                    _lookup['/'] = true;
                    _lookup['?'] = true;
                    _lookup['`'] = true;
                    _lookup['{'] = true;
                    _lookup['|'] = true;
                    _lookup['}'] = true;
                    _lookup['~'] = true;
                    _lookup['<'] = true;
                    _lookup['>'] = true;

                    // Set Lookup to Defined 
                    IsLookupDefined = true;
                }

                return _lookup;
            }
        }

        /// <summary>
        /// Remove any special characters from the String
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string RemoveSpecialCharacters(string inputString)
        {
            // Uses a StringBuilder to speed up the process
            StringBuilder _sb = new StringBuilder(inputString.Length);

            // Check each individual character of the list and only keep valid characters ([0-9], [@], [A-Z], [a-z], [.])
            for (int _iPosition = 0; _iPosition < inputString.Length; _iPosition++)
            {
                char _c = inputString[_iPosition];

                if (_c < 65530)
                {
                    if (Lookup[_c])
                        _sb.Append(_c);
                }
            }

            return _sb.ToString();
        }

        #endregion Special Character Treatment
    }
}
