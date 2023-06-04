using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SqlTypes;

namespace Laba3
{
    /*на экран числа (результат спени)
     * блок меьнше модуля!!
     * все шифры симметричные
     * 
     * РСА
     * п и кью простые, при расшифр брать по 16 бит(можно дополнить), п на кью > 65536,
     * закрытый ввести самому(2 условия. простой и простой с функцие эйлера)
     * возведение, эвклид
     * вывод чисел(в 10 системе)
     * 
     * ЭЛЬгамаль
     * жэ проверить(в состальных остаток не равен 1)
     * возведение 
     * все первообразные найти, и вывести с их количеством, пользователь выбирает первообразную
     * к взаимно простое 
     * а и бэ разделить(а не менятеся, бэ да)
     * Рабин    
     * проверка на простоту , п и кью делятся на 4  с состатком 3 вроде
     * бэ с клаввы < м
     * расш нужно решить квадр уравнение, корень из дискр, можно по какой то китайской теореме
     * п кью по эвклиду, 4 дискр и 4 корня, выбрать правильный : от 0 до 255
     * 
     * знать степень, эйлера  3 штуку, свое число для этого и это в отчет
     * 
     * ВАРИАНТ 1 (по 4 лабе вариант 2)
     */
    public partial class MainWindow : Window
    {
        private byte[] sourceBytes = null;
        private short[] sourceShorts = null;
        private byte[] resultBytes = null;
        private short[] resultShorts = null;
        bool encrypt;
        int p = 0, q = 0, d = 0, r = 0, f = 0, e = 0;

        public MainWindow()
        {
            InitializeComponent();
        }

        private string arrayToString(byte[] bytes)
        {
            string res = "";     
            int len = bytes.Length;
            if (bytes.Length > 64)
            {
                len = 64;
            }
            for (int i = 0; i < len; i++)
            {
                res += bytes[i].ToString() + " ";
            }
            return res;
        }

        private void rbEncryptChecked(object sender, RoutedEventArgs e)
        {
            encrypt = true;
            tbP.IsEnabled = true;
            tbQ.IsEnabled = true;
            tbE.IsEnabled = true;
            tbR.IsEnabled = false;
            tbD.IsEnabled = false;
        }

        private void rbDecryptChecked(object sender, RoutedEventArgs e)
        {
            encrypt = false;
            tbR.IsEnabled = true;
            tbD.IsEnabled = true;
            tbP.IsEnabled = false;
            tbQ.IsEnabled = false;
            tbE.IsEnabled = false;
        }

        private void btnClickOpenFile(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                sourceBytes = File.ReadAllBytes(openFileDialog.FileName);
                //tbSourceText.Text = File.ReadAllText(openFileDialog.FileName);
                tbSourceText.Text = arrayToString(sourceBytes);
                tbResultText.Text = "";
            }
        }

        private bool prime(int x)
        {
            for (int i = 2; i <= Math.Sqrt(x); i++)
            {
                if (x % i == 0)
                {
                    return false;
                }
            }
            return true;
        }

        private bool primes(int a, int b)
        {
            while (a != 0 && b != 0)
            {
                if (a > b)
                {
                    a = a % b;
                 
                }
                else
                {
                    b = b % a;
                }
            }
            if (a + b == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private int Evclid(int a, int b)
        {
            int d0 = a, d1 = b, d2, x0 = 1, x1 = 0, x2, y0 = 0, y1 = 1, y2, q;
            while (d1 > 1)
            {
                q = d0 / d1;
                d2 = d0 % d1;
                x2 = x0 - q * x1;
                y2 = y0 - q * y1;
                d0 = d1;
                d1 = d2;
                x0 = x1;
                x1 = x2;
                y0 = y1;
                y1 = y2;
            }
            if (y1 < 0)
            {
                y1 += a;
            }
            return y1;
        }

        private bool checkInput()
        {
            if (sourceBytes == null)
            {
                MessageBox.Show("Файл не открыт!");
                return false;
            }
            if (encrypt)
            {
                if (!(int.TryParse(tbP.Text, out p) && prime(p)))
                {
                    MessageBox.Show("p введено некорректно!");
                    return false;
                }
                if (!(int.TryParse(tbQ.Text, out q) && prime(q)))
                {
                    MessageBox.Show("q введено некорректно!");
                    return false;
                }
                if (p * q < 256)
                {
                    MessageBox.Show("p и q слишком маленькие!");
                    return false;
                }
                if (p * q > 256 * 256)
                {
                    MessageBox.Show("p и q слишком большие!");
                    return false;
                }

                f = (p - 1) * (q - 1);
                //MessageBox.Show(f.ToString());
                if (!(int.TryParse(tbE.Text, out e) && e > 1 && e < f && primes(e, f)))
                {
                    MessageBox.Show("e введено некорректно!");
                    return false;
                }

                r = p * q;
                d = Evclid(f, e);

                tbR.Text = r.ToString();
                tbD.Text = d.ToString();
            }
            else
            {
                if (!int.TryParse(tbR.Text, out r))
                {
                    MessageBox.Show("r введено некорректно!");
                    return false;
                }
                if (!int.TryParse(tbD.Text, out d))
                {
                    MessageBox.Show("d введено некорректно!");
                    return false;
                }
            }
            return true;
        }

        void BytesToShortsArray()
        {
            sourceShorts = new short[sourceBytes.Length / 2];
            int j = 0;
            short x;
            for (int i = 0; i < sourceBytes.Length - 1; i += 2)
            {
                x = sourceBytes[i];
                x = (short)(x << 8);
                x += sourceBytes[i + 1];
                sourceShorts[j] = x;
                j++;
            }
            if (sourceBytes.Length % 2 == 1)
            {
                x = sourceBytes[sourceBytes.Length - 1];
                x = (short)(x << 8);
                sourceShorts[j] = x;
            }
        }

        void ShortsToBytesArray()
        {
            resultBytes = new byte[resultShorts.Length * 2];
            int j = 0;
            byte x;
            for (int i = 0; i < resultShorts.Length; i++)
            {
                x = (byte)((resultShorts[i] >> 8));
                resultBytes[j] = x;
                j++;
                x = (byte)(resultShorts[i] & 255);                
                resultBytes[j] = x;
                j++;
            }
        }

        private short PowModule(int a, int z, int m)
        {
            int x = 1;
            while (z != 0)
            {
                while (z % 2 == 0)
                {
                    z = z / 2;
                    a = (a * a) % m;
                }
                z--;
                x = (x * a) % m;
            }           
            return (short)x;
        }

        private void Encrypt()
        {
            resultShorts = new short[sourceBytes.Length];
            for (int i = 0; i < sourceBytes.Length; i++)
            {
                resultShorts[i] = PowModule(sourceBytes[i], e, r);
            }
            ShortsToBytesArray();
        }

        void Decrypt()
        {
            BytesToShortsArray();
            resultBytes = new byte[sourceShorts.Length];
            for (int i = 0; i < sourceShorts.Length; i++)
            {
                resultBytes[i] = (byte)PowModule(sourceShorts[i], d, r);
            }
        }

        private void btnClickSaveFile(object sender, RoutedEventArgs e)
        {
            if (checkInput())
            {                        
                if (encrypt)
                {                                     
                    Encrypt();                    
                }
                else
                {               
                    Decrypt();
                }                
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                if (saveFileDialog.ShowDialog() == true)
                { 
                    File.WriteAllBytes(saveFileDialog.FileName, resultBytes);
                    //tbResultText.Text = File.ReadAllText(saveFileDialog.FileName);
                    tbResultText.Text = arrayToString(resultBytes);
                    sourceBytes = null;
                    sourceShorts = null;
                    resultBytes = null;
                    resultShorts = null;
                }
            }
        }
    }
}
