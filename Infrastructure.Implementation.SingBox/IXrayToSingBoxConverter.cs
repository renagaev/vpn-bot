using Infrastructure.Implementation.SingBox.SingBoxModels;
using Infrastructure.Implementation.SingBox.XrayModels;

namespace Infrastructure.Implementation.SingBox;

public interface IXrayToSingBoxConverter
{
    /// <summary>
    /// Конвертирует xray конфигурацию в singbox формат
    /// </summary>
    /// <param name="xrayConfig">Xray конфигурация</param>
    /// <returns>SingBox конфигурация</returns>
    SingBoxConfig Convert(XrayConfigRoot xrayConfig);
    
    /// <summary>
    /// Конвертирует JSON строку xray конфигурации в singbox формат
    /// </summary>
    /// <param name="xrayJson">JSON строка xray конфигурации</param>
    /// <returns>JSON строка singbox конфигурации</returns>
    string ConvertJson(string xrayJson);
    
    /// <summary>
    /// Конвертирует массив xray конфигураций в одну singbox конфигурацию с объединением всех outbounds и rules
    /// </summary>
    /// <param name="xrayJsonArray">JSON массив xray конфигураций</param>
    /// <returns>JSON строка singbox конфигурации</returns>
    string ConvertJsonArray(string xrayJsonArray);
}
