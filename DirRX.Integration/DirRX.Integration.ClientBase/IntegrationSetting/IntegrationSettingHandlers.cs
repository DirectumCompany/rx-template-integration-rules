using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.Integration.IntegrationSetting;

namespace DirRX.Integration
{
  partial class IntegrationSettingRulesClientHandlers
  {

    public virtual void RulesExecutionOrderValueInput(Sungero.Presentation.IntegerValueInputEventArgs e)
    {
      if (e.NewValue == e.OldValue)
        return;
      
      if (e.NewValue < 1)
        e.AddError(DirRX.Integration.IntegrationSettings.Resources.ExecutionOrderShouldBePositive);
      
      if (_obj.IntegrationSetting.Rules.Select(x => x.ExecutionOrder).Contains(e.NewValue))
        e.AddError(DirRX.Integration.IntegrationSettings.Resources.ExecutionOrderShouldBeUniq);
    }
  }


  partial class IntegrationSettingClientHandlers
  {

    public override void Refresh(Sungero.Presentation.FormRefreshEventArgs e)
    {
      Functions.IntegrationSetting.SetRequiredProperties(_obj);
    }

  }
}