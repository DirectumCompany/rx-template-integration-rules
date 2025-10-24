using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.Integration.Shared
{
  public partial class ModuleFunctions
  {
    
    /// <summary>
    /// Экранирование спецсимволов XML.
    /// </summary>
    /// <param name="toXml">Строка для вставки в xml.</param>
    /// <returns>Строка с экранированными спецсимволами.</returns>
    [Public]
    public static string EscapeXml(string toXml)
    {
      if (!string.IsNullOrWhiteSpace(toXml))
      {
        toXml = toXml.Replace("&", "&amp;");
        toXml = toXml.Replace("'", "&apos;");
        toXml = toXml.Replace("\"", "&quot;");
        toXml = toXml.Replace(">", "&gt;");
        toXml = toXml.Replace("<", "&lt;");
      }
      return toXml;
    }
    
    /// <summary>
    /// Замена спецсимволов XML.
    /// </summary>
    /// <param name="unXml">Строка с экранированными спецсимволами.</param>
    /// <returns>Строка со спецсимволами XML.</returns>
    [Public]
    public static string UnescapeXml(string unXml)
    {
      if (!string.IsNullOrWhiteSpace(unXml))
      {
        unXml = unXml.Replace("&apos;", "'");
        unXml = unXml.Replace("&quot;", "\"");
        unXml = unXml.Replace("&gt;", ">");
        unXml = unXml.Replace("&lt;", "<");
        unXml = unXml.Replace("&amp;", "&");
      }
      return unXml;
    }
    
  }
}