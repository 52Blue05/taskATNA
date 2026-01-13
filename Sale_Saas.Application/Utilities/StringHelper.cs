using Sale_Saas.Application.Common.Models;
using Sale_Saas.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Sale_Saas.Application.Utilities;

public class StringHelper
{
    public string Left(string param, int length)
    {
        //we start at 0 since we want to get the characters starting from the
        //left and with the specified lenght and assign it to a variable
        string result = param.Substring(0, length);
        //return the result of the operation
        return result;
    }
    public string Right(string param, int length)
    {
        //start at the index based on the lenght of the sting minus
        //the specified lenght and assign it a variable
        string result = param.Substring(param.Length - length, length);
        //return the result of the operation
        return result;
    }

    public string Mid(string param, int startIndex, int length)
    {
        //start at the specified index in the string ang get N number of
        //characters depending on the lenght and assign it to a variable
        string result = param.Substring(startIndex, length);
        //return the result of the operation
        return result;
    }

    public string Mid(string param, int startIndex)
    {
        //start at the specified index and return all characters after it
        //and assign it to a variable
        string result = param.Substring(startIndex);
        //return the result of the operation
        return result;
    }
    public static string convertToUnSign(string s)
    {
        try
        {
			Regex regex = new Regex("\\p{IsCombiningDiacriticalMarks}+");
			string temp = s.Normalize(NormalizationForm.FormD);
			return regex.Replace(temp, String.Empty).Replace('\u0111', 'd').Replace('\u0110', 'D');
		}
        catch(Exception ex)
        {
            return s;
        }
    }

    public static string BoDauVaKhoangTrang(string s)
    {
		s = s.Replace(" ", "");
        s = convertToUnSign(s);
        return s;
	}

    public static string BoDauVaLayKyTuDau(string s)
    {
        string result = string.Empty;
        string temp = convertToUnSign(s);
        if (temp.IndexOf(' ') > -1)
        {
            string[] temps = temp.Split();
            foreach (string item in temps)
            {
                result += item[0];
            }
        }
        else
        {
            result = temp;
        }

        return result;
    }
    public static string ChuyenSo(string number)
    {
        return replace_special_word(join_unit(number)).ToUpper().Trim();
    }

    private static string join_unit(string n)
    {
        int sokytu = n.Length;
        int sodonvi = (sokytu % 3 > 0) ? (sokytu / 3 + 1) : (sokytu / 3);
        n = n.PadLeft(sodonvi * 3, '0');
        sokytu = n.Length;
        string chuoi = "";
        int i = 1;
        while (i <= sodonvi)
        {
            if (i == sodonvi) chuoi = join_number((int.Parse(n.Substring(sokytu - (i * 3), 3))).ToString()) + unit(i) + chuoi;
            else chuoi = join_number(n.Substring(sokytu - (i * 3), 3)) + unit(i) + chuoi;
            i += 1;
        }
        return chuoi;
    }

    private static string replace_special_word(string chuoi)
    {
        chuoi = chuoi.Replace("không mươi không ", "");
        chuoi = chuoi.Replace("không trăm ", "");
        chuoi = chuoi.Replace("không mươi", "lẻ");
        chuoi = chuoi.Replace("i không", "i");
        chuoi = chuoi.Replace("i năm", "i lăm");
        chuoi = chuoi.Replace("một mươi", "mười");
        chuoi = chuoi.Replace("mươi một", "mươi mốt");
        chuoi = chuoi.Replace("triệu nghìn", "triệu");
        return chuoi;
    }
  
    private static string unit(int n)
    {
        string chuoi = "";
        if (n == 1) chuoi = " đồng ";
        else if (n == 2) chuoi = " nghìn ";
        else if (n == 3) chuoi = " triệu ";
        else if (n == 4) chuoi = " tỷ ";
        else if (n == 5) chuoi = " nghìn tỷ ";
        else if (n == 6) chuoi = " triệu tỷ ";
        else if (n == 7) chuoi = " tỷ tỷ ";
        return chuoi;
    }

    private static string convert_number(string n)
    {
        string chuoi = "";
        if (n == "0") chuoi = "không";
        else if (n == "1") chuoi = "một";
        else if (n == "2") chuoi = "hai";
        else if (n == "3") chuoi = "ba";
        else if (n == "4") chuoi = "bốn";
        else if (n == "5") chuoi = "năm";
        else if (n == "6") chuoi = "sáu";
        else if (n == "7") chuoi = "bảy";
        else if (n == "8") chuoi = "tám";
        else if (n == "9") chuoi = "chín";
        return chuoi;
    }

    private static string join_number(string n)
    {
        string chuoi = "";
        int i = 1, j = n.Length;
        while (i <= j)
        {
            if (i == 1) chuoi = convert_number(n.Substring(j - i, 1)) + chuoi;
            else if (i == 2) chuoi = convert_number(n.Substring(j - i, 1)) + " mươi " + chuoi;
            else if (i == 3) chuoi = convert_number(n.Substring(j - i, 1)) + " trăm " + chuoi;
            i += 1;
        }
        return chuoi;
    }


    public static string convertCurrencyVND(double chuoi, bool isShowCurrency = true, string currencyCode = "")
    {
        var info = System.Globalization.CultureInfo.GetCultureInfo("vi-VN");
        return isShowCurrency ? chuoi.ToString("#,###", info.NumberFormat) + currencyCode : chuoi.ToString("#,###");
    }

    public static string convertCurrency(double chuoi)
    {
        var formatInfo = (NumberFormatInfo)CultureInfo.CurrentCulture.NumberFormat.Clone();

        formatInfo.CurrencyDecimalDigits = 0;
        formatInfo.NumberDecimalDigits = 0;
        formatInfo.NumberGroupSeparator = ".";
        return chuoi.ToString("N", formatInfo);
    }

    public static string FirstCharToUpper(string source)
    {
        if (string.IsNullOrEmpty(source))
            return string.Empty;
        // convert to char array of the string
        char[] letters = source.Trim().ToCharArray();
        // upper case the first char
        letters[0] = char.ToUpper(letters[0]);
        // return the array made of the new char array
        return new string(letters);
    }

    public static string GenerateRandomCode(int length, int maxRange)
    {
        Random random = new Random();
        int value = random.Next(maxRange);

        string mark = "";

        if (length > 0)
        {
            for (int i = 0; i < length; i++)
            {
                mark += "0";
            }
        }
        else
        {
            mark = "0000";
        }

        return value.ToString(mark);
    }

    public static DateTime? ToDatetime(string date)
    {
        try
        {
            string format = "dd/MM/yyyy";
            DateTime dateTime = DateTime.ParseExact(date, format, CultureInfo.InvariantCulture);
            return dateTime;
        }
        catch(Exception ex)
        {
            return null;
        }
    }


    public static string[] mNumText = "không;một;hai;ba;bốn;năm;sáu;bảy;tám;chín".Split(';');
    public static string[] mNumTextEN = "zero;one;two;three;four;five;six;seven;eight;nine".Split(';');
    public static string[] mNumTextHangChucEN = "zero;one;twenty;thirty;forty;fifty;sixty;seventy;eighty; ninety".Split(';');
    private static string DocHangChuc(double so, bool daydu)
    {
        string chuoi = "";
        //Hàm để lấy số hàng chục ví dụ 21/10 = 2
        Int64 chuc = Convert.ToInt64(Math.Floor((double)(so / 10)));
        //Lấy số hàng đơn vị bằng phép chia 21 % 10 = 1
        Int64 donvi = (Int64)so % 10;
        //Nếu số hàng chục tồn tại tức >=20
        if (chuc > 1)
        {
            chuoi = " " + mNumText[chuc] + " mươi";
            if (donvi == 1)
            {
                chuoi += " mốt";
            }
        }
        else if (chuc == 1)
        {//Số hàng chục từ 10-19
            chuoi = " mười";
            if (donvi == 1)
            {
                chuoi += " một";
            }
        }
        else if (daydu && donvi > 0)
        {//Nếu hàng đơn vị khác 0 và có các số hàng trăm ví dụ 101 => thì biến daydu = true => và sẽ đọc một trăm lẻ một
            chuoi = " lẻ";
        }
        if (donvi == 5 && chuc >= 1)
        {//Nếu đơn vị là số 5 và có hàng chục thì chuỗi sẽ là " lăm" chứ không phải là " năm"
            chuoi += " lăm";
        }
        else if (donvi > 1 || (donvi == 1 && chuc == 0))
        {
            chuoi += " " + mNumText[donvi];
        }
        return chuoi;
    }
    private static string DocHangChucEN(double so, bool daydu)
    {
        string chuoi = "";
        //Hàm để lấy số hàng chục ví dụ 21/10 = 2
        Int64 chuc = Convert.ToInt64(Math.Floor((double)(so / 10)));
        //Lấy số hàng đơn vị bằng phép chia 21 % 10 = 1
        Int64 donvi = (Int64)so % 10;
        //Nếu số hàng chục tồn tại tức >=20
        if (chuc > 1)
        {
            chuoi = " " + mNumTextHangChucEN[chuc];
            chuoi += donvi > 0 ? " " + mNumTextEN[donvi] : "";
        }
        else if (chuc == 1)
        {//Số hàng chục từ 10-19
            switch (donvi)
            {
                case 0:
                    chuoi += " ten";
                    break;
                case 1:
                chuoi += " eleven";
                    break;
                case 2:
                    chuoi += " twelve";
                    break;
                case 3:
                    chuoi += " thirteen";
                    break;
                case 4:
                    chuoi += " fourteen";
                    break;
                case 5:
                    chuoi += " fifteen";
                    break;
                case 6:
                    chuoi += " sixteen";
                    break;
                case 7:
                    chuoi += " seventeen";
                    break;
                case 8:
                    chuoi += " eighteen";
                    break;
                case 9:
                    chuoi += " nineteen";
                    break;
                default:
                    break;
            }
        }
        else if (donvi > 1 || (donvi == 1 && chuc == 0))
        {
            chuoi += " " + mNumTextEN[donvi];
        }
        return chuoi;
    }
    public static string DocHangTram(double so, bool daydu)
    {
        string chuoi = "";
        //Lấy số hàng trăm ví du 434 / 100 = 4 (hàm Floor sẽ làm tròn số nguyên bé nhất)
        Int64 tram = Convert.ToInt64(Math.Floor((double)so / 100));
        //Lấy phần còn lại của hàng trăm 434 % 100 = 34 (dư 34)
        so = so % 100;
        if (daydu || tram > 0)
        {
            chuoi = " " + mNumText[tram] + " trăm";
            chuoi += DocHangChuc(so, true);
        }
        else
        {
            chuoi = DocHangChuc(so, false);
        }
        return chuoi;
    }
    public static string DocHangTramEN(double so, bool daydu)
    {
        string chuoi = "";
        //Lấy số hàng trăm ví du 434 / 100 = 4 (hàm Floor sẽ làm tròn số nguyên bé nhất)
        Int64 tram = Convert.ToInt64(Math.Floor((double)so / 100));
        //Lấy phần còn lại của hàng trăm 434 % 100 = 34 (dư 34)
        so = so % 100;
        if (tram > 0)
        {
            chuoi = " " + mNumTextEN[tram] + " hundred";
            chuoi += DocHangChucEN(so, true);
        }
        else
        {
            chuoi = DocHangChucEN(so, false);
        }
        return chuoi;
    }
    public static string DocHangTrieu(double so, bool daydu)
    {
        string chuoi = "";
        //Lấy số hàng triệu
        Int64 trieu = Convert.ToInt64(Math.Floor((double)so / 1000000));
        //Lấy phần dư sau số hàng triệu ví dụ 2,123,000 => so = 123,000
        so = so % 1000000;
        if (trieu > 0)
        {
            chuoi = DocHangTram(trieu, daydu) + " triệu";
            daydu = true;
        }
        //Lấy số hàng nghìn
        Int64 nghin = Convert.ToInt64(Math.Floor((double)so / 1000));
        //Lấy phần dư sau số hàng nghin
        so = so % 1000;
        if (nghin > 0)
        {
            chuoi += DocHangTram(nghin, daydu) + " nghìn";
            daydu = true;
        }
        if (so > 0)
        {
            chuoi += DocHangTram(so, daydu);
        }
        return chuoi;
    }
    public static string DocHangTrieuEN(double so, bool daydu)
    {
        string chuoi = "";
        //Lấy số hàng triệu
        Int64 trieu = Convert.ToInt64(Math.Floor((double)so / 1000000));
        //Lấy phần dư sau số hàng triệu ví dụ 2,123,000 => so = 123,000
        so = so % 1000000;
        if (trieu > 0)
        {
            chuoi = DocHangTramEN(trieu, daydu) + " million";
            daydu = true;
        }
        //Lấy số hàng nghìn
        Int64 nghin = Convert.ToInt64(Math.Floor((double)so / 1000));
        //Lấy phần dư sau số hàng nghin
        so = so % 1000;
        if (nghin > 0)
        {
            chuoi += DocHangTramEN(nghin, daydu) + " thousand";
            daydu = true;
        }
        if (so > 0)
        {
            chuoi += DocHangTramEN(so, daydu);
        }
        return chuoi;
    }
    public static string ChuyenSoSangChuoi(double so)
    {
        if (so == 0)
            return "không";
        string chuoi = "", hauto = "";
        Int64 ty;
        do
        {
            //Lấy số hàng tỷ
            ty = Convert.ToInt64(Math.Floor((double)so / 1000000000));
            //Lấy phần dư sau số hàng tỷ
            so = so % 1000000000;
            if (ty > 0)
            {
                chuoi = DocHangTrieu(so, true) + hauto + chuoi;
            }
            else
            {
                chuoi = DocHangTrieu(so, false) + hauto + chuoi;
            }
            hauto = " tỷ";
        } while (ty > 0);
        return chuoi + " đồng";
    }
    public static string ChuyenSoSangChuoiEN(double so)
    {
        if (so == 0)
            return "";
        string chuoi = "", hauto = "";
        Int64 ty;
        do
        {
            //Lấy số hàng tỷ
            ty = Convert.ToInt64(Math.Floor((double)so / 1000000000));
            //Lấy phần dư sau số hàng tỷ
            so = so % 1000000000;
            if (ty > 0)
            {
                chuoi = DocHangTrieuEN(so, true) + hauto + chuoi;
            }
            else
            {
                chuoi = DocHangTrieuEN(so, false) + hauto + chuoi;
            }
            hauto = " billion";
        } while (ty > 0);
        return chuoi + " dong";
    }

    public static bool IsValidEmail(string email)
    {
        if (!string.IsNullOrEmpty(email))
        {
            return Regex.IsMatch(email, @"^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$");

        }
        else
        {
            return false;
        }
    }
    public static string ToRoman(int number)
    {
        if ((number < 0) || (number > 3999)) throw new ArgumentOutOfRangeException("insert value betwheen 1 and 3999");
        if (number < 1) return string.Empty;
        if (number >= 1000) return "M" + ToRoman(number - 1000);
        if (number >= 900) return "CM" + ToRoman(number - 900);
        if (number >= 500) return "D" + ToRoman(number - 500);
        if (number >= 400) return "CD" + ToRoman(number - 400);
        if (number >= 100) return "C" + ToRoman(number - 100);
        if (number >= 90) return "XC" + ToRoman(number - 90);
        if (number >= 50) return "L" + ToRoman(number - 50);
        if (number >= 40) return "XL" + ToRoman(number - 40);
        if (number >= 10) return "X" + ToRoman(number - 10);
        if (number >= 9) return "IX" + ToRoman(number - 9);
        if (number >= 5) return "V" + ToRoman(number - 5);
        if (number >= 4) return "IV" + ToRoman(number - 4);
        if (number >= 1) return "I" + ToRoman(number - 1);
        throw new ArgumentOutOfRangeException("something bad happened");
    }

    public static bool IsNearlyInteger(double number)
    {
        return Math.Round(number, 2) == Math.Round(number);
    }



    private static SymmetricAlgorithm GetAlgorithm(string password)
    {
        var algorithm = Rijndael.Create();
        var rdb = new Rfc2898DeriveBytes(password, new byte[] {
            0x53,0x6f,0x64,0x69,0x75,0x6d,0x20,             // salty goodness
            0x43,0x68,0x6c,0x6f,0x72,0x69,0x64,0x65
                });
        algorithm.Padding = PaddingMode.ISO10126;
        algorithm.Key = rdb.GetBytes(32);
        algorithm.IV = rdb.GetBytes(16);
        return algorithm;
    }


    public static string EncryptString(string base64ChuaMaHoa, string password)
    {
        string res = string.Empty;
        if (!string.IsNullOrEmpty(base64ChuaMaHoa))
        {
            var algorithm = GetAlgorithm(password);
            var encryptor = algorithm.CreateEncryptor();
            var clearBytes = Encoding.Unicode.GetBytes(base64ChuaMaHoa);
            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(clearBytes, 0, clearBytes.Length);
                cs.Close();
                res = Convert.ToBase64String(ms.ToArray());
            }
        }

        return res;
    }

    public static string DecryptString(string base64MaHoa, string password)
    {
        string res = string.Empty;
        if (!string.IsNullOrEmpty(base64MaHoa))
        {
            var algorithm = GetAlgorithm(password);
            var decryptor = algorithm.CreateDecryptor();
            var cipherBytes = Convert.FromBase64String(base64MaHoa);
            using (var ms = new MemoryStream())
            using (var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Write))
            {
                cs.Write(cipherBytes, 0, cipherBytes.Length);
                cs.Close();
                res = Encoding.Unicode.GetString(ms.ToArray());
            }

        }
        return res;
    }

    public static string RandomString(int length)
    {
        Random random = new Random();

        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }

    public static string GetMessage(string locale,string vn, string us)
    {
        if(locale == LocaleEnum.en_US.ToString())
        {
            return us;
        }
        else
        {
            return vn;
        }
    }

	public static string GenerateCode()
	{
        try
        {
			DateTime today = DateTime.Today;
			string datePart = today.ToString("ddMMyy");
			string randomString = GenerateRandomString(6).ToUpper();
			string finalCode = $"{datePart}-{randomString}";
			return finalCode;
		}
        catch(Exception ex)
        {
            return GenerateRandomString(6);
		}
	}

	public static string GetFullName(string? FirstName, string? LastName)
	{
		try
		{
            return (LastName ?? "") + " " + (FirstName ?? "");
		}
		catch (Exception ex)
		{
            return "";
		}
	}

	public static string GenerateRandomString(int length)
	{
		const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
		Random random = new Random();
		char[] result = new char[length];

		for (int i = 0; i < length; i++)
		{
			result[i] = chars[random.Next(chars.Length)];
		}

		return new string(result);
	}
	public static string DictGetValue(Dictionary<string,string> dict,string key)
	{
		try
		{
			return dict.ContainsKey(key) ? dict[key] : "";
		}
		catch(Exception ex)
		{
			return "";
        }
	}

	public static bool IsValidLength(int length,string? s = "",bool? isThrow = false)
	{
		try
		{
            bool result = true;
            if (!string.IsNullOrEmpty(s) && s.Length > length)
            {
                result = false;
				if (isThrow == true)
				{
					throw new ApplicationException($"Độ dài kí tự không hợp lệ, tối đa chỉ {length} kí tự");
				}
			}
            else
            {
                result = false;
            }
            return result;
		}
		catch (Exception ex)
		{
            if (isThrow == true)
            {
                throw new ApplicationException($"Độ dài kí tự không hợp lệ, tối đa chỉ {length} kí tự");
            }
			return false;
		}
	}

	public static bool IsNumberOutOfRange(string? number = "",bool? isThrow = false)
	{
        try
        {
            bool result = false;
			if (string.IsNullOrEmpty(number))
			{
                return result;
			}

			bool isInteger = int.TryParse(number, out int intValue);
			bool isLong = long.TryParse(number, out long longValue);
			bool isFloat = float.TryParse(number, out float floatValue);
			bool isDouble = double.TryParse(number, out double doubleValue);
			bool isDecimal = decimal.TryParse(number, out decimal decimalValue);

			if (isInteger && intValue > int.MaxValue)
			{
				if (isThrow == true)
				{
					throw new ApplicationException($"Độ dài kí tự không hợp lệ, tối đa chỉ {int.MaxValue} kí tự");
				}
				return true;
			}
			if (isLong && longValue > long.MaxValue)
			{
				if (isThrow == true)
				{
					throw new ApplicationException($"Độ dài kí tự không hợp lệ, tối đa chỉ {long.MaxValue} kí tự");
				}
				return true;
			}
			if (isFloat && floatValue > float.MaxValue)
			{
				if (isThrow == true)
				{
					throw new ApplicationException($"Độ dài kí tự không hợp lệ, tối đa chỉ {float.MaxValue} kí tự");
				}
				return true;
			}
			if (isDouble && doubleValue > double.MaxValue)
			{
				if (isThrow == true)
				{
					throw new ApplicationException($"Độ dài kí tự không hợp lệ, tối đa chỉ {double.MaxValue} kí tự");
				}
				return true;
			}
			if (isDecimal && decimalValue > decimal.MaxValue)
			{
				if (isThrow == true)
				{
					throw new ApplicationException($"Độ dài kí tự không hợp lệ, tối đa chỉ {decimal.MaxValue} kí tự");
				}
				return true;
			}

            

			return false;
		}
        catch(Exception ex)
        {
			if (isThrow == true)
			{
				throw new ApplicationException(ex.Message);
			}
			return false;
        }
		
	}

}
