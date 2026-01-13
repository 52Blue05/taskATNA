using Sale_Saas.Domain.Constants.API;
using Sale_Saas.Application.Interfaces.Services;
using System.Globalization;

namespace Sale_Saas.Infrastructure.Services;

public class InternalService : IInternalService
{
    private readonly DataTypeFormatConstant _dataTypeFormatConstant;

    public InternalService(IOptions<DataTypeFormatConstant> dataTypeFormatConstant)
    {
        _dataTypeFormatConstant = dataTypeFormatConstant.Value;
    }
    public object MapValueToObject(object oProperty,
                                                    Dictionary<string, string> data,
                                                    object updateObj)
    {
        int intValue;
        double doubleValue;
        bool boolValue;
        DateTime ngayValue;
        Guid guidValue;

        if(updateObj != null)
        {
            foreach (KeyValuePair<string, string> updateItem in data)
            {
                if (oProperty.GetType().GetProperty(updateItem.Key) != null)
                {
                    if (oProperty.GetType().GetProperty(updateItem.Key).PropertyType == typeof(string))
                    {
                        updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, updateItem.Value, null);
                    }
                    else if (oProperty.GetType().GetProperty(updateItem.Key).PropertyType == typeof(Guid))
                    {
                        if (Guid.TryParse(updateItem.Value, out guidValue))
                        {
                            updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, guidValue, null);
                        }
                        else
                        {
                            throw new ApplicationException($"Giá trị của {updateItem.Key}: {updateItem.Value} không phải là Guid");
                        }
                    }
                    else if (oProperty.GetType().GetProperty(updateItem.Key).PropertyType == typeof(Guid?))
                    {
                        if (!string.IsNullOrEmpty(updateItem.Value))
                        {
                            if (Guid.TryParse(updateItem.Value, out guidValue))
                            {
                                updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, guidValue, null);
                            }
                            else
                            {
                                throw new ApplicationException($"Giá trị của {updateItem.Key}: {updateItem.Value} không phải là Guid");
                            }
                        }
                        else
                        {
                            updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, null, null);
                        }
                    }
                    else if (oProperty.GetType().GetProperty(updateItem.Key).PropertyType == typeof(int))
                    {
                        if (int.TryParse(updateItem.Value, out intValue))
                        {
                            updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, intValue, null);
                        }
                        else
                        {
                            throw new ApplicationException($"Giá trị của {updateItem.Key}: {updateItem.Value} không phải là Int");
                        }
                    }
                    else if (oProperty.GetType().GetProperty(updateItem.Key).PropertyType == typeof(int?))
                    {
                        if (!string.IsNullOrEmpty(updateItem.Value))
                        {
                            if (int.TryParse(updateItem.Value, out intValue))
                            {
                                updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, intValue, null);
                            }
                            else
                            {
                                throw new ApplicationException($"Giá trị của {updateItem.Key}: {updateItem.Value} không phải là Int");
                            }
                        }
                        else
                        {
                            updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, null, null);
                        }
                    }
                    else if (oProperty.GetType().GetProperty(updateItem.Key).PropertyType == typeof(double))
                    {
                        if (double.TryParse(updateItem.Value, out doubleValue))
                        {
                            updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, doubleValue, null);
                        }
                        else
                        {
                            throw new ApplicationException($"Giá trị của {updateItem.Key}: {updateItem.Value} không phải là double");
                        }
                    }
                    else if (oProperty.GetType().GetProperty(updateItem.Key).PropertyType == typeof(double?))
                    {
                        if (!string.IsNullOrEmpty(updateItem.Value))
                        {
                            if (double.TryParse(updateItem.Value, out doubleValue))
                            {
                                updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, doubleValue, null);
                            }
                            else
                            {
                                throw new ApplicationException($"Giá trị của {updateItem.Key}: {updateItem.Value} không phải là double");
                            }
                        }
                        else
                        {
                            updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, null, null);
                        }
                    }
                    else if (oProperty.GetType().GetProperty(updateItem.Key).PropertyType == typeof(bool))
                    {
                        if (bool.TryParse(updateItem.Value, out boolValue))
                        {
                            updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, boolValue, null);
                        }
                        else
                        {
                            throw new ApplicationException($"Giá trị của {updateItem.Key}: {updateItem.Value} không phải là bool");
                        }
                    }
                    else if (oProperty.GetType().GetProperty(updateItem.Key).PropertyType == typeof(bool?))
                    {
                        if (!string.IsNullOrEmpty(updateItem.Value))
                        {
                            if (bool.TryParse(updateItem.Value, out boolValue))
                            {
                                updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, boolValue, null);
                            }
                            else
                            {
                                throw new ApplicationException($"Giá trị của {updateItem.Key}: {updateItem.Value} không phải là bool");
                            }
                        }
                        else
                        {
                            updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, null, null);
                        }
                    }
                    else if (oProperty.GetType().GetProperty(updateItem.Key).PropertyType == typeof(DateTime))
                    {
                        if (DateTime.TryParseExact(updateItem.Value, _dataTypeFormatConstant.DateFormat, null, DateTimeStyles.None, out ngayValue))
                        {
                            updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, ngayValue, null);
                        }
                        else
                        {
                            throw new ApplicationException($"Giá trị của {updateItem.Key}: {updateItem.Value} không đúng định dạng: {_dataTypeFormatConstant.DateFormat}");
                        }
                    }
                    else if (oProperty.GetType().GetProperty(updateItem.Key).PropertyType == typeof(DateTime?))
                    {
                        if (!string.IsNullOrEmpty(updateItem.Value))
                        {
                            if (DateTime.TryParseExact(updateItem.Value, _dataTypeFormatConstant.DateFormat, null, DateTimeStyles.None, out ngayValue))
                            {
                                updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, ngayValue, null);
                            }
                            else
                            {
                                throw new ApplicationException($"Giá trị của {updateItem.Key}: {updateItem.Value} không đúng định dạng: {_dataTypeFormatConstant.DateFormat}");
                            }
                        }
                        else
                        {
                            updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, null, null);
                        }
                    }
                    // ==============================
                    else if (oProperty.GetType().GetProperty(updateItem.Key).PropertyType == typeof(TimeSpan))
                    {
                        TimeSpan timeSpanValue;
                        if (TimeSpan.TryParseExact(updateItem.Value, "d\\.hh\\:mm\\:ss", CultureInfo.InvariantCulture, out timeSpanValue))
                        {
                            updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, timeSpanValue, null);
                        }
                        else
                        {
                            throw new ApplicationException($"Giá trị của {updateItem.Key}: {updateItem.Value} không đúng định dạng cho TimeSpan");
                        }
                    }
                    else if (oProperty.GetType().GetProperty(updateItem.Key).PropertyType == typeof(TimeSpan?))
                    {
                        TimeSpan timeSpanValue;
                        if (!string.IsNullOrEmpty(updateItem.Value))
                        {
                            if (TimeSpan.TryParseExact(updateItem.Value, "d\\.hh\\:mm\\:ss", CultureInfo.InvariantCulture, out timeSpanValue))
                            {
                                updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, timeSpanValue, null);
                            }
                            else
                            {
                                throw new ApplicationException($"Giá trị của {updateItem.Key}: {updateItem.Value} không đúng định dạng cho TimeSpan");
                            }
                        }
                        else
                        {
                            updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, null, null);
                        }
                    }
                    else if (oProperty.GetType().GetProperty(updateItem.Key).PropertyType == typeof(decimal))
                    {
                        decimal decimalValue;
                        if (decimal.TryParse(updateItem.Value, out decimalValue))
                        {
                            updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, decimalValue, null);
                        }
                        else
                        {
                            throw new ApplicationException($"Giá trị của {updateItem.Key}: {updateItem.Value} không đúng định dạng cho Decimal");
                        }
                    }
                    else if (oProperty.GetType().GetProperty(updateItem.Key).PropertyType == typeof(decimal?))
                    {
                        decimal decimalValue;
                        if (!string.IsNullOrEmpty(updateItem.Value))
                        {
                            if (decimal.TryParse(updateItem.Value, out decimalValue))
                            {
                                updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, decimalValue, null);
                            }
                            else
                            {
                                throw new ApplicationException($"Giá trị của {updateItem.Key}: {updateItem.Value} không đúng định dạng cho Decimal");
                            }
                        }
                        else
                        {
                            updateObj.GetType().GetProperty(updateItem.Key).SetValue(updateObj, null, null);
                        }
                    }
                }
                //else
                //{
                //    throw new ApplicationException($"Không tìm thấy field {updateItem.Key}");
                //}
            }
        }        

        return updateObj;
    }
}
