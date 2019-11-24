using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace HelloApps.GUI
{

    public class GUIUtils
    {
        public static CommandEditorClass GetEditorInstance(Control current_instance)
        {
            if (current_instance == null)
                return null;

            if (current_instance.GetType().Equals(typeof(CommandEditorClass)))
                return (CommandEditorClass)current_instance;

            if (current_instance.Parent == null)
                return null;

            Control parent_control = current_instance.Parent;

            while (parent_control != null && !parent_control.GetType().Equals(typeof(CommandEditorClass)))
            {
                if (parent_control.Parent != null)
                    parent_control = parent_control.Parent;
                else
                    break;
            }

            if (parent_control.GetType().Equals(typeof(CommandEditorClass)))
                return (CommandEditorClass)parent_control;
            else
                return null;
        }

        public static string[] SetSafeArrayValue(string[] arr, string value)
        {
            return SetSafeArrayValue(arr, 0, value);
        }


        public static string[] SetSafeArrayValue(string[] arr, int pos, string value)
        {
            if (arr != null && pos < arr.Length)
                arr[pos] = value;

            return arr;
        }

        public static string GetSafeArrayValue(string[] arr)
        {
            return GetSafeArrayValue(arr, 0);
        }

        public static string GetSafeArrayValue(string[] arr, int pos)
        {
            if (arr != null && pos < arr.Length)
                return arr[pos];
            else
                return string.Empty;
        }


        public static string[] CreateStringArray(int size)
        {
            string[] arr = new string[size];

            for (int i = 0; i < size; i++)
                arr[i] = string.Empty;

            return arr;
        }

        public static string[] CreateStringArray(object p1)
        {
            string[] arr = new string[1];

            if (p1 == null)
                arr[0] = string.Empty;
            else
                arr[0] = p1.ToString();

            return arr;
        }

        public static string[] CreateStringArray(object p1, object p2)
        {
            string[] arr = new string[2];

            if (p1 == null)
                arr[0] = string.Empty;
            else
                arr[0] = p1.ToString();

            if (p2 == null)
                arr[1] = string.Empty;
            else
                arr[1] = p2.ToString();

            return arr;
        }

        public static string[] CreateStringArray(object p1, object p2, object p3)
        {
            string[] arr = new string[3];

            if (p1 == null)
                arr[0] = string.Empty;
            else
                arr[0] = p1.ToString();

            if (p2 == null)
                arr[1] = string.Empty;
            else
                arr[1] = p2.ToString();

            if (p3 == null)
                arr[2] = string.Empty;
            else
                arr[2] = p3.ToString();

            return arr;
        }


        public static string[] CreateStringArray(object p1, object p2, object p3, object p4)
        {
            string[] arr = new string[4];

            if (p1 == null)
                arr[0] = string.Empty;
            else
                arr[0] = p1.ToString();

            if (p2 == null)
                arr[1] = string.Empty;
            else
                arr[1] = p2.ToString();

            if (p3 == null)
                arr[2] = string.Empty;
            else
                arr[2] = p3.ToString();

            if (p4 == null)
                arr[3] = string.Empty;
            else
                arr[3] = p4.ToString();

            return arr;
        }


        public static string[] CreateStringArray(object p1, object p2, object p3, object p4, object p5)
        {
            string[] arr = new string[5];

            if (p1 == null)
                arr[0] = string.Empty;
            else
                arr[0] = p1.ToString();

            if (p2 == null)
                arr[1] = string.Empty;
            else
                arr[1] = p2.ToString();

            if (p3 == null)
                arr[2] = string.Empty;
            else
                arr[2] = p3.ToString();

            if (p4 == null)
                arr[3] = string.Empty;
            else
                arr[3] = p4.ToString();

            if (p5 == null)
                arr[4] = string.Empty;
            else
                arr[4] = p5.ToString();

            return arr;
        }

        public static string[] GetStringArray(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            string[] arr = str.Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            return arr;
        }

        public static string[] GetStringArray(string str1, string str2)
        {
            if (string.IsNullOrEmpty(str1) && string.IsNullOrEmpty(str2))
                return null;


            string[] arr1 = null;
            string[] arr2 = null;

            int arr1_size = 0;
            int arr2_size = 0;

            if (!string.IsNullOrEmpty(str1))
            {
                arr1 = str1.Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                arr1_size = arr1.Length;
            }

            if (!string.IsNullOrEmpty(str2))
            {
                arr2 = str2.Split(new char[] { ' ', ',', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                arr2_size = arr2.Length;
            }

            string[] arr = new string[arr1_size + arr2_size];

            int ind = 0;

            for (int i = 0; i < arr1_size; i++)
            {
                arr[ind] = arr1[i];
                ind++;
            }

            for (int i = 0; i < arr2_size; i++)
            {
                arr[ind] = arr2[i];
                ind++;
            }

            return arr;
        }


        public static float[] GetFloatArray(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            string[] arr = GetStringArray(str);

            if (arr != null)
            {
                float[] vars = new float[arr.Length];

                for (int i = 0; i < arr.Length; i++)
                {
                    vars[i] = float.Parse(arr[i]);
                }

                return vars;
            }
            else
                return null;
        }


        public static int[] GetIntArray(string str)
        {
            if (string.IsNullOrEmpty(str))
                return null;

            string[] arr = GetStringArray(str);

            if (arr != null)
            {
                int[] vars = new int[arr.Length];

                for (int i = 0; i < arr.Length; i++)
                {
                    vars[i] = int.Parse(arr[i]);
                }

                return vars;
            }
            else
                return null;
        }

        public static string GetStringFromFloatArray(float[] vars)
        {
            string res = string.Empty;

            if (vars != null)
            {
                for (int i = 0; i < vars.Length; i++)
                {
                    if (i < (vars.Length - 1))
                        res = res + vars[i].ToString() + "  ";
                    else
                        res = res + vars[i].ToString();
                }
            }

            return res;
        }

        public static float[] CloneFloatArray(float[] vars)
        {
            if (vars == null)
                return null;

            float[] new_vals = new float[vars.Length];

            for (int i = 0; i < vars.Length; i++)
            {
                new_vals[i] = vars[i];
            }

            return new_vals;
        }

        public static float[] GetFloatArray(float x, float y, float z)
        {
            float[] new_vals = new float[3];

            new_vals[0] = x;
            new_vals[1] = y;
            new_vals[2] = z;

            return new_vals;
        }

        public static bool IsChanged_FloatArray(float[] op1, float[] op2)
        {
            //Equal True
            //Differ False
            bool res = false;

            if (op1 == null || op2 == null)
                return false;

            if (op1.Length != op2.Length)
                return false;

            for (int i = 0; i < op1.Length; i++)
            {
                if (op1[i] != op2[i])
                {
                    res = true;
                    break;
                }
            }

            return res;
        }

        public static Color CloneColor(Color org)
        {
            Color new_color = Color.FromArgb(org.R, org.G, org.B);
            return new_color;
        }
    }

}
