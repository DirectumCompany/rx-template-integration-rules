using System;
using System.Collections.Generic;
using System.Linq;
using Sungero.Core;
using Sungero.CoreEntities;

namespace DirRX.Integration.Structures.ImportRuleDepartment
{
  partial class Department
  {
    public string Name {get; set;}
    public string ExternalId {get; set;}
    public string BusinessUnitCode {get; set;}
    public string HeadOfficeExternalId {get; set;}
    public string ManagerExternalId {get; set;}
    public Sungero.Core.Enumeration Status {get; set;}
  }
}