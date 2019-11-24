using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Win32;

namespace HelloApps.Common
{

    public class Util
    {

        public static string ReplaceSlashString(string src)
        {
            char[] arr = src.ToCharArray();

            int openInd = src.IndexOf('"', 0);
            int closeInd = -1;

            while (openInd >= 0)
            {
                closeInd = src.IndexOf('"', openInd + 1);

                if (closeInd >= 0)
                {
                    for (int i = openInd; i <= closeInd; i++)
                    {
                        if (arr[i] == '/')
                            arr[i] = '|';
                    }

                    openInd = src.IndexOf('"', closeInd + 1);
                }
                else
                    openInd = -1;
            }

            return new string(arr).Trim();
        }

        public static string RestoreSlashString(string str)
        {
            string res = str.Replace('|', '/').Trim();

            if (res.StartsWith("\""))
                res = res.Substring(1, res.Length - 1);

            if (res.EndsWith("\""))
                res = res.Substring(0, res.Length - 1);

            return res;
        }


		public static string RestoreSingleQuatString(string str)
		{
			if (str == string.Empty)
				return string.Empty;
			
			string res = string.Empty;

			for (int i = 0; i < str.Length; i++)
			{
				if (str[i] == (char)0x02)
					res += '\'';
                else if (str[i] == (char)0x03)
                    res += '<';
                else if (str[i] == (char)0x04)
                    res += '>';
				else
					res += str[i];
			}
			
			return res;
		}


    }
}
