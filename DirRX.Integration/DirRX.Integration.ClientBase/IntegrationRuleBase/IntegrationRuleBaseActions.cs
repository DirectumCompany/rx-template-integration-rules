using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.Integration.IntegrationRuleBase;

namespace DirRX.Integration.Client
{
  partial class IntegrationRuleBaseActions
  {
    public override void DeleteEntity(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.DeleteEntity(e);
    }

    public override bool CanDeleteEntity(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return false;
    }

    public virtual void ExecuteIntegration(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      Logger.DebugFormat("Test");
    }

    public virtual bool CanExecuteIntegration(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
    }

  }


}