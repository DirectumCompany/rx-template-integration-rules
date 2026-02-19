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
    public virtual void SetPassword(Sungero.Domain.Client.ExecuteActionArgs e)
    {      
      var dialog = Dialogs.CreateInputDialog(DirRX.Integration.IntegrationSettings.Resources.DialogPassword);
      var password = dialog.AddPasswordString(DirRX.Integration.IntegrationSettings.Resources.DialogPassword, true);
      dialog.Buttons.AddOkCancel();
      dialog.Buttons.Default = DialogButtons.Ok;
      dialog.SetOnButtonClick(a =>
                              {
                                if (a.Button == DialogButtons.Ok)
                                {
                                  if (!string.IsNullOrEmpty(password.Value))
                                    _obj.Password = Sungero.ExchangeCore.PublicFunctions.BusinessUnitBox.GetEncryptedData(password.Value);
                                }
                              });
      dialog.Show();
    }

    public virtual bool CanSetPassword(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return true;
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