using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.Integration.IntegrationSetting;

namespace DirRX.Integration.Server
{
  partial class IntegrationSettingFunctions
  {
    /// <summary>
    /// Получить исполнителя или группу исполнителей.
    /// </summary>
    /// <returns>Исполнитель или группа исполнителей.</returns>
    public virtual Sungero.CoreEntities.IRecipient GetResponsibles()
    {
      return _obj.Responsible;
    }

  }
}