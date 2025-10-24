using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;
using DirRX.Integration.IntegrationSetting;

namespace DirRX.Integration.Client
{

  partial class IntegrationSettingActions
  {
    public override void DeleteEntity(Sungero.Domain.Client.ExecuteActionArgs e)
    {
      base.DeleteEntity(e);
    }

    public override bool CanDeleteEntity(Sungero.Domain.Client.CanExecuteActionArgs e)
    {
      return false;
    }

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

  }

}