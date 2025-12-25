namespace Sale_Saas.Application.Interfaces.Services;

public interface IInternalService
{
    object MapValueToObject(object oProperty, Dictionary<string, string> data, object updateObj);
}
