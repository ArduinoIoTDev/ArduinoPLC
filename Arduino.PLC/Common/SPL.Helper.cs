using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace HelloApps.Common
{
    public class ParsingHelper
    {
        public static ArrayList GetStringList(string inputStr)
        {
            string[] arr;
            arr = inputStr.Split(new Char[] { ' ', ',', '\t' }, 200);

            ArrayList newArr = new ArrayList();

            for (int i = 0; i < arr.Length; i++)
            {
                if (arr[i] != string.Empty)
                {
                    newArr.Add(arr[i]);
                }
            }


            return newArr;
        }
               

        public static string GetFirstToken(string line)
        {
            if (line == string.Empty)
                return string.Empty; ;


            string firstToken = string.Empty;

            ArrayList tokenList = GetStringList(line);

            if (tokenList.Count > 0)
                firstToken = tokenList[0].ToString(); 

            if (firstToken.IndexOf("=") >= 0)
            {
                int eqInd = firstToken.IndexOf("=");

                if (eqInd == 0)
                    firstToken = string.Empty;
                else
                    firstToken = firstToken.Substring(0, eqInd);
            }

            if (firstToken.IndexOf("(") >= 0)
            {
                int eqInd = firstToken.IndexOf("(");

                if (eqInd == 0)
                    firstToken = string.Empty;
                else
                    firstToken = firstToken.Substring(0, eqInd);
            }

            if (firstToken.IndexOf(":") >= 0)
            {
                int eqInd = firstToken.IndexOf(":");

                if (eqInd == 0)
                    firstToken = string.Empty;
                else
                    firstToken = firstToken.Substring(0, eqInd);
            }

            if (firstToken.IndexOf("+") >= 0)
            {
                int eqInd = firstToken.IndexOf("+");

                if (eqInd == 0)
                    firstToken = string.Empty;
                else
                    firstToken = firstToken.Substring(0, eqInd);
            }

            if (firstToken.IndexOf("-") >= 0)
            {
                int eqInd = firstToken.IndexOf("-");

                if (eqInd == 0)
                    firstToken = string.Empty;
                else
                    firstToken = firstToken.Substring(0, eqInd);
            }

            if (firstToken.IndexOf("{") == 0)
                firstToken = "{";
            else if (firstToken.IndexOf("}") == 0)
                firstToken = "}";
            else
            {
                if (firstToken.IndexOf("{") >= 0)
                {
                    int eqInd = firstToken.IndexOf("{");
                    firstToken = firstToken.Substring(0, eqInd);
                }
            }

            return firstToken;
        }
    }

}


