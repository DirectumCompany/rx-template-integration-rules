using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.Integration.IntegrationRuleBase;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace DirRX.Integration.Server
{
  partial class IntegrationRuleBaseFunctions
  {
    
    #region Общие функции.
    
    /// <summary>
    /// Выполнить интеграцию.
    /// </summary>
    /// <param name="integrationSettings">Настройки интеграции.</param>
    /// <param name="logs">Структурированный лог.</param>
    public virtual void ExecuteIntegration(DirRX.Integration.IIntegrationSetting integrationSettings, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      var parameters = this.CreateRequestParameters();
      var response = Functions.IntegrationRuleBase.ExecuteConnector(_obj, integrationSettings, parameters, logs);
      this.ProcessResponse(response, logs);
    }
    
    /// <summary>
    /// Сформировать параметры для запроса к внешней системе.
    /// </summary>
    /// <returns>Словарь параметров.</returns>
    public virtual System.Collections.Generic.Dictionary<string, string> CreateRequestParameters()
    {
      var parameters = new System.Collections.Generic.Dictionary<string, string>();
      foreach (var item in _obj.Parameters)
        parameters.Add(item.Parameter, item.Value);
      
      return parameters;
    }
    
    /// <summary>
    /// Обработать ответ от внешней системы.
    /// </summary>
    /// <param name="response">Матрица с ответом от внешней системы.</param>
    /// <param name="logs">Структурированный лог.</param>
    public virtual void ProcessResponse(List<System.Collections.Generic.Dictionary<string, string>> response, List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      throw new NotImplementedException();
    }
    
    /// <summary>
    /// Создать external link для импортированных данных из внешней системы.
    /// </summary>
    /// <param name="entity">Сущность.</param>
    /// <param name="entityTypeGuid">GUID типа сущности.</param>
    /// <param name="externalEntityId">Внешний ID экземпляра.</param>
    [Public]
    public static Sungero.Domain.Shared.IExternalLink CreateExternalLink(Sungero.Domain.Shared.IEntity entity, Guid entityTypeGuid, string externalEntityId)
    {
      var externalLink = Sungero.Domain.ModuleFunctions.CreateExternalLink();
      externalLink.EntityTypeGuid = entityTypeGuid;
      externalLink.ExternalEntityId = externalEntityId;
      externalLink.ExternalSystemId = DirRX.Integration.Constants.IntegrationRuleBase.ExternalLinkSystem;
      externalLink.EntityId = entity.Id;
      externalLink.IsDeleted = false;
      externalLink.Save();
      return externalLink;
    }
    
    /// <summary>
    /// Получить расшифрованные данные.
    /// </summary>
    /// <param name="encryptedData">Зашифрованные данные.</param>
    /// <returns>Расшифрованные данные.</returns>
    [Remote(IsPure = true)]
    public static string GetDecryptedData(string encryptedData)
    {
      return Sungero.Core.Encryption.Decrypt(encryptedData);
    }
    
    #endregion
    
    #region Коннектор к внешней системе.
    
    /// <summary>
    /// Выполнить обращение к внешней системе с использованием коннектора.
    /// </summary>
    /// <param name="integrationSettings">Настройки интеграции.</param>
    /// <param name="parameters">Словарь параметров.</param>
    /// <param name="logs">Структурированный лог.</param>
    /// <returns>Матрица с ответом от внешней системы.</returns>
    /// <remarks>Добавлена возможность перекрытия в случае необходимости добавить другие способы обмена.</remarks>
    public virtual List<System.Collections.Generic.Dictionary<string, string>> ExecuteConnector(DirRX.Integration.IIntegrationSetting integrationSettings,
                                                                                                System.Collections.Generic.Dictionary<string, string> parameters,
                                                                                                List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      var response = new List<Dictionary<string, string>>();
      if (parameters.Any())
      {
        if (integrationSettings.ConnectorType == DirRX.Integration.IntegrationSetting.ConnectorType.DefaultConnector)
          response = this.ExecuteDefaultConnector(integrationSettings, parameters, logs);
      }
      return response;
    }
    
    #region Реализация коннектора по умолчанию.

    /// <summary>
    /// Выполнить обращение к внешней системе с использованием SOAP Connector.
    /// </summary>
    /// <param name="integrationSettings">Настройки интеграции.</param>
    /// <param name="parameters">Словарь параметров.</param>
    /// <param name="logs">Структурированный лог.</param>
    /// <returns>Матрица с ответом от внешней системы.</returns>
    public virtual List<System.Collections.Generic.Dictionary<string, string>> ExecuteDefaultConnector(DirRX.Integration.IIntegrationSetting integrationSettings,
                                                                                                       System.Collections.Generic.Dictionary<string, string> parameters,
                                                                                                       List<DirRX.Integration.Structures.Module.LogStruct> logs)
    {
      var result = new List<Dictionary<string, string>>();
      var response = string.Empty;
      
      try
      {
        response = SendDefaultRequest(_obj.Uri, integrationSettings.Login,
                                      GetDecryptedData(integrationSettings.Password), _obj.ActionName, parameters);
      }
      catch (Exception ex)
      {
        var errorMessage = string.Format("Ошибка при выполнении коннектора по умолчанию. Подробности: {0}", ex.Message);
        Logger.Error(errorMessage, ex);
        logs.Add(DirRX.Integration.Functions.Module.CreateLogItem(_obj.Name,
                                                                  DirRX.Integration.Constants.Module.Logging.MessageLevel.RequestLevel,
                                                                  DirRX.Integration.Constants.Module.Logging.MessageType.Error,
                                                                  errorMessage));
      }
      
      try
      {
        if (!string.IsNullOrEmpty(response))
          result = ParseDefaultResponse(response);
      }
      catch (Exception ex)
      {
        var errorMessage = string.Format("Ошибка при разборе ответа от коннектора по умолчанию. Подробности: {0}.", ex.Message);
        Logger.Error(errorMessage, ex);
        logs.Add(DirRX.Integration.Functions.Module.CreateLogItem(_obj.Name,
                                                                  DirRX.Integration.Constants.Module.Logging.MessageLevel.ResponseLevel,
                                                                  DirRX.Integration.Constants.Module.Logging.MessageType.Error,
                                                                  errorMessage));
      }
      
      
      return result;
    }
    
    /// <summary>
    /// Подготовка SOAP конверта.
    /// </summary>
    /// <param name="action">Наименование действия.</param>
    /// <param name="parameters">Словарь параметров.</param>
    /// <returns>SOAP конверт.</returns>
    public static System.Xml.XmlDocument CreateSoapEnvelope(string action, System.Collections.Generic.Dictionary<string, string> parameters)
    {
      var soapEnvelopeXml = new XmlDocument();
      soapEnvelopeXml.PreserveWhitespace = true;  // По умолчанию false и если передается пробел (например <IEdc> </IEdc>), то тэги раздедяются переносом строк.
      var xmlStr = @"<?xml version=""1.0"" encoding=""utf-8""?>
                    <soap12:Envelope xmlns:soap12=""http://www.w3.org/2003/05/soap-envelope""
                      xmlns:urn=""urn:sap-com:document:sap:soap:functions:mc-style""
                      xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
                      xmlns:xsd=""http://www.w3.org/2001/XMLSchema"">
                      <soap12:Header/>
                      <soap12:Body>
                        <urn:{0}>{1}</urn:{0}>
                      </soap12:Body>
                    </soap12:Envelope>";
      
      var filledParameters = parameters.Where(x => !string.IsNullOrEmpty(x.Value)).Select(x => string.Format("<{0}>{1}</{0}>", x.Key, x.Value)).ToArray();
      var emptyParameters = parameters.Where(x => string.IsNullOrEmpty(x.Value)).Select(x => string.Format("<{0}/>", x.Key)).ToArray();
      var allParams = filledParameters.Concat(emptyParameters);
      var parametersBlock = string.Join(string.Empty, allParams);

      var soapRequestString = string.Format(xmlStr, action, parametersBlock);
      soapEnvelopeXml.LoadXml(soapRequestString);
      return soapEnvelopeXml;
    }
    
    /// <summary>
    /// Игнорировать самоподписанные и просроченные сертификаты.
    /// </summary>
    public static void IgnoreBadCertificates()
    {
      System.Net.ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(AcceptAllCertifications);
    }
    
    /// <summary>
    /// Принимать все сертификаты.
    /// </summary>
    /// <returns>Для всех случаев возвращаем "True".</returns>
    public static bool AcceptAllCertifications(object sender,
                                               System.Security.Cryptography.X509Certificates.X509Certificate certification,
                                               System.Security.Cryptography.X509Certificates.X509Chain chain,
                                               System.Net.Security.SslPolicyErrors sslPolicyErrors)
    {
      return true;
    }

    /// <summary>
    /// Выполняет отправку запроса на целевой адрес и возвращает ответ.
    /// </summary>
    /// <param name="urlEndpont">URL-адрес конечной точки.</param>
    /// <param name="userName">Имя пользователя.</param>
    /// <param name="userPassword">Пароль.</param>
    /// <param name="action">Наименование действия.</param>
    /// <param name="parameters">Словарь параметров.</param>
    /// <returns>Строка, которая содержит ответ от веб-сервиса.</returns>
    [Public]
    public static string SendDefaultRequest(string urlEndpoint,
                                            string userName,
                                            string userPassword,
                                            string action,
                                            System.Collections.Generic.Dictionary<string, string> parameters)
    {
      IgnoreBadCertificates();
      ServicePointManager.Expect100Continue = true;
      ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
        | SecurityProtocolType.Tls11
        | SecurityProtocolType.Tls12;
      
      XmlDocument soapEnvelopeXml;
      try
      {
        soapEnvelopeXml = CreateSoapEnvelope(action, parameters);
      }
      catch (Exception ex)
      {
        Logger.Error("Ошибка при формировании SOAP конверта", ex);
        throw AppliedCodeException.Create(DirRX.Integration.IntegrationSettings.Resources.ErrorCreateSoapEnvelope);
      }
      
      #region Отправка запроса.
      HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(urlEndpoint);

      webRequest.Credentials = new NetworkCredential(userName, userPassword);
      webRequest.ContentType = "application/soap+xml;charset=\"utf-8\"";
      webRequest.Accept = "application/soap+xml";
      webRequest.Method = "POST";

      // Insert SOAP envelope.
      using (Stream stream = webRequest.GetRequestStream())
      {
        soapEnvelopeXml.Save(stream);
      }

      // Send request and retrieve result.
      string result;
      try
      {
        using (WebResponse response = webRequest.GetResponse())
        {
          using (StreamReader rd = new StreamReader(response.GetResponseStream()))
          {
            result = rd.ReadToEnd();
            return result;
          }
        }
      }
      catch (WebException webex)
      {
        // TODO При необходимости дописать обработку других статусов.
        if (webex.Status == WebExceptionStatus.Timeout)
        {
          throw AppliedCodeException.Create(DirRX.Integration.IntegrationSettings.Resources.HttpStatusTimeout);
        }
        if (webex.Status == System.Net.WebExceptionStatus.ProtocolError)
        {
          var webResponse = webex.Response as System.Net.HttpWebResponse;
          if (webResponse != null)
          {
            Logger.ErrorFormat("Ошибка при отправке. Код ответа HTTP: {0} - {1} - {2}", (int)webResponse.StatusCode, webResponse.StatusCode, webResponse.StatusDescription);
            // TODO При необходимости дописать обработку других кодов ответа HTTP.
            if (webResponse.StatusCode == System.Net.HttpStatusCode.Unauthorized)
              throw AppliedCodeException.Create(DirRX.Integration.IntegrationSettings.Resources.HttpStatusCode401Unauthorized);
          }
        }
        
        WebResponse errResp = webex.Response;
        var errorMessage = string.Empty;
        var getResponse = false;
        if (errResp != null)
        {
          using (Stream respStream = errResp.GetResponseStream())
          {
            // Может вернуть как xml так и html.
            StreamReader reader = new StreamReader(respStream);
            errorMessage = reader.ReadToEnd();
            getResponse = true;
          }
        }
        else
        {
          // Попытка получить хоть какие-то подробности об ошибке, например таймаут.
          errorMessage += " Status=" + webex.Status;
          errorMessage += " Message=" + webex.Message;
        }
        var message = string.Format("Удаленный сервер вернул ошибку: {0}", errorMessage);
        Logger.Error(message, webex);
        // Если получен ответ от внешней системы - дать возможность обработать ответ другим функциям, иначе выбросить исключение.
        if (getResponse)
          return errorMessage;
        else
          throw AppliedCodeException.Create(message);
      }
      #endregion
    }
    
    /// <summary>
    /// Выполняет обработку строки с ответом и преобразует в структурированный формат.
    /// </summary>
    /// <param name="response">Строка, содержащая SOAP-ответ.</param>
    /// <returns>Ответ в виде списка массивов строк.</returns>
    public virtual List<System.Collections.Generic.Dictionary<string, string>> ParseDefaultResponse(string response)
    {
      var result = new List<Dictionary<string, string>>();
      var responseXml = XDocument.Parse(response);
      foreach (var block in responseXml.Descendants(_obj.ResultTagName))
      {
        if (block != null && block.HasElements)
        {
          foreach (var element in block.Elements())
          {
            var tempDictionary = new Dictionary<string, string>();
            foreach (var el in element.Elements())
              tempDictionary.Add(el.Name.LocalName, el.Value);
            result.Add(tempDictionary);
          }
        }
      }
      return result;
    }
    
    #endregion
    
    #endregion
    
  }
}