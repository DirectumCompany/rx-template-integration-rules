using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.Integration.IntegrationSetting;

namespace DirRX.Integration.Shared
{
  partial class IntegrationSettingFunctions
  {
    /// <summary>
    /// Настройка доступности свойств по умолчанию.
    /// </summary>
    /// <remarks>
    /// Т.к. мы программного создаем запись, и создаем ее с пустыми полями. То обязательность полей должна задаваться при открытии карточки.
    /// </remarks>
    public virtual void SetRequiredProperties()
    {
      #region Основные настройки отображения.
      _obj.State.Properties.ConnectorType.IsRequired = true;
      _obj.State.Properties.BusinessUnits.IsRequired = true;
      _obj.State.Properties.Responsible.IsRequired = true;
      _obj.State.Properties.Rules.IsRequired = true;
      _obj.State.Properties.Login.IsRequired = true;
      _obj.State.Properties.Password.IsRequired = true;
      #endregion
    }
  }
}