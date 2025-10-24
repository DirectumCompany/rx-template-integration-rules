using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.Integration.IntegrationRuleBase;

namespace DirRX.Integration.Shared
{
  partial class IntegrationRuleBaseFunctions
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
      _obj.State.Properties.Port.IsRequired = true;
      _obj.State.Properties.Uri.IsRequired = true;
      _obj.State.Properties.ActionName.IsRequired = true;
      _obj.State.Properties.ResultTagName.IsRequired = true;
      #endregion
    }
    
  }
}