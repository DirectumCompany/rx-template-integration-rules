# **Правила интеграции с внешними системами**
Репозиторий с шаблоном разработки «**Правила интеграции с внешними системами**».
## **Описание**
Решение предоставляет механизм настройки правил интеграции по расписанию и в определенном порядке.

Состав объектов разработки:
* фоновый процесс «Выполнить интеграцию с внешней системой»;
* справочник «Настройки интеграции» (IntegrationSetting);
* абстрактный справочник «Базовое правило интеграции»(IntegrationRuleBase);
* абстрактный справочник «Базовое правило импорта»(ImportRuleBase);
* справочник «Правило импорта подразделений» (ImportRuleDepartment);
* справочник «Правило импорта сотрудников» (ImportRuleEmployee);
* справочник «Правило импорта должностей» (ImportRuleJobTitle);

В справочнике «**Настройки интеграции**» можно задать параметры для подключения к внешней системе, список правил импорта для этой системы и порядок их выполнения.

В справочниках правил задается Uri для обращения во внешнюю систему и параметры запроса.

Фоновый процесс по расписанию получает все действующие настройки и выполняет импорт из внешней системы по порядку из настройки. Результаты импорта отправляются ответственным.
В шаблоне реализован импорт в Directum RX справочников Подразделения, Должности и Сотрудники.

> [!NOTE]
> Замечания и пожеланию по развитию шаблона разработки фиксируйте через [Issues](https://github.com/DirectumCompany/rx-template-integration-rules/issues).
При оформлении ошибки, опишите сценарий для воспроизведения. Для пожеланий приведите обоснование для описываемых изменений - частоту использования, бизнес-ценность, риски и/или эффект от реализации.
>
> Внимание! Изменения будут вноситься только в новые версии.

## Варианты расширения функциональности на проектах
Существуют следующие варианты расширения:
* создание или изменение правила интеграции;
* добавление нового тип коннектора;
* вызов интеграции из событий сущностей, блоков задач, действий и т.д.

### Поддержка нового типа коннектора
В качестве примера рассмотрим тип коннектора, который имитирует обмен данными с внешней системой без подключения к ней. Например, это можно использовать при отладке кода.
1. Перекройте справочник IntegrationSetting.
2. Типы коннекторов задаются в справочнике IntegrationSetting в свойстве ConnectorType. Добавьте значение перечисления, которое соответствует новому типу коннектора, например NewConnector:
3. Переопределите серверную функцию ExecuteConnector() и добавьте обработку нового типа коннектора подключения:
```
/// <summary>
/// Выполнить обращение к внешней системе.
/// </summary>
/// <param name="parameters">Словарь параметров.</param>
/// <param name="connectorType">Тип коннектора подключения.</param>
/// <param name="logs">Структурированный лог.</param>
/// <returns>Матрица с ответом от внешней системы.</returns>
public override List<string[]> ExecuteConnector(DirRX.Integration.IIntegrationRuleBase rule, System.Collections.Generic.Dictionary<string, string> parameters, List<DirRX.Integration.Structures.Module.LogStruct> logs)
{
  var response = base.ExecuteConnector(rule, parameters, logs);
  if (parameters.Any())
  {
    if (_obj.ConnectorType == DirRX.Dummy.IntegrationSetting.ConnectorType.DummyProvider)
      response = this.ExecuteDummyProvider(rule, parameters, logs);
  }
  return response;
}
```
4. Создайте функции, которые имитируют запросы к внешней системе и обрабатывают полученные значения. Тестовые данные в явном виде прописаны в функциях GetTestDataDepartments(), GetTestDataEmployees(), GetTestDataJobTitles(). В функции ExecuteDummyProvider() заполняется матрица ответа от внешней системы данными из перечисленных функций.
```
/// <summary>
/// Имитация работы с внешней системой.
/// </summary>
/// <param name="rule">Правило интеграции.</param>
/// <param name="parameters">Словарь параметров.</param>
/// <param name="logs">Структурированный лог.</param>
/// <returns>Матрица с ответом от внешней системы.</returns>
public virtual List<string[]> ExecuteDummyProvider(DirRX.Integration.IIntegrationRuleBase rule, System.Collections.Generic.Dictionary<string, string> parameters, List<DirRX.Integration.Structures.Module.LogStruct> logs)
{
  var response = new List<string[]>();
  #region Заполнение данных в матрице ответа от внешней системы («заглушка»).
  if (parameters["method"] != null)
  {
    if (parameters["method"] == "GetDepartments")
      response = GetTestDataDepartments();
    else if (parameters["method"] == "GetEmployees")
      response = GetTestDataEmployees();
    else if (parameters["method"] == "GetJobTitles")
      response = GetTestDataJobTitles();
  }
  #endregion
  return response;
}
```
5. Опубликуйте решение и проверьте внесенные изменения.
6. В веб-клиенте Directum RX в записи справочника Настройки синхронизации с внешней системой в раскрывающемся списке Тип коннектора выберите пункт с добавленным коннектором. Затем укажите остальные настройки синхронизации и проверьте ее работоспособность.

### Создание или изменение правила интеграции
Правило интеграции представляет собой тип справочника с набором прикладных функций. Правило отправляет запрос из одной системы в другую и обрабатывает полученный ответ.
В шаблоне предполагается, что внешняя система возвращает ответ в виде двумерного массива (матрицы), в котором колонки соответствуют параметрам, а строки – значениям параметров.
В среде разработки Directum RX после установки решения IntegrationRulesSolution появляется модуль Integration с набором правил. Схема наследования правил: 
<img width="637" height="254" alt="image" src="https://github.com/user-attachments/assets/0a7de1af-ff0e-411c-ad59-24b0ceccc2ba" />
* IntegrationRuleBase – базовое правило интеграции;
* ImportRuleBase – базовое правило импорта данных из внешней системы в Directum RX;
* ImportRuleDepartment – импорт подразделений в Directum RX;
* ImportRuleJobTitle – импорт должностей в Directum RX;
* ImportRuleEmployee – импорт сотрудников в Directum RX.
Если нужно реализовать экспорт данных из внешней системы в Directum RX, создайте правило экспорта. При этом в качестве базового правила используйте IntegrationRuleBase.
Чтобы реализовать импорт новой сущности в Directum RX, создайте правило, в качестве базового правила используйте ImportRuleBase.
Если требуется изменить правило импорта подразделений, должностей или сотрудников, то перекройте нужное правило, например ImportRuleDepartment, и переопределите его функции.

### Создание правила импорта
Общий порядок создания правила импорта:
1. В своем решении создайте наследника от базового правила ImportRuleBase.
2. Переопределите серверные функции базового правила с учетом решаемой задачи:
   * SaveData() – сохранение полученных данных в Directum RX;
   * ParseResponseItem() – обработка строки матрицы с ответом от внешней системы.
3. Создайте серверную функцию, например ImportEntities(), которая импортирует данные в сущность Directum RX. Создайте структуру, которая содержит набор полей сущности. Созданная функция импорта будет вызываться из функции SaveData().
4. Переопределите функцию инициализации, чтобы при ее выполнении создавались свои правила помимо базовых.

#### Пример создания правила для импорта контрагентов
1. Создайте правило для импорта контрагентов ImportRuleCounterparty. В качестве базового правила укажите ImportRuleBase.
2. В созданном правиле ImportRuleCounterparty создайте структуру CounterpartyStructure, которая содержит набор полей сущности.
```
/// <summary>
/// Структура для работы с контрагентами.
/// </summary
partial class CounterpartyStructure
{
  public string Name { get; set; }
  public string Tin { get; set; }
  public string Psrn { get; set; }
}
```
3. Переопределите серверную функцию SaveData() базового правила.
```
/// <summary>
/// Сохранение полученных данных в Directum RX.
/// </summary>
/// <param name="response">Матрица с ответом от внешней системы.</param>
/// <param name="logs">Структурированный лог.</param>
public override void SaveData(List<string[]> response, List<DirRX.Solution.Structures.Module.LogStruct> logs)
{
  var entities = new List<DirRX.Solution.Structures.ImportRuleCounterparty.CounterpartyStructure>();
  foreach (var responseItem in response)
    entities.Add(ParseResponseItem(responseItem));
  if (entities.Any())
  {
    ImportEntities(entities, logs);
  }
}
```
4. Переопределите серверную функцию ParseResponseItem() базового правила.
```
/// <summary>
/// Обработка строки матрицы с ответом от внешней системы.
/// </summary>
/// <param name="responseItem">Строка матрицы с ответом от внешней системы.</param>
/// <returns>Свойства сущности в структурированном виде.</returns>
public virtual DirRX.Solution.Structures.ImportRuleCounterparty.CounterpartyStructure ParseResponseItem(string[] responseItem)
{
  var entity = new
  DirRX.Solution.Structures.ImportRuleCounterparty.CounterpartyStructure();
  entity.Name = responseItem[0];
  entity.Tin = responseItem[1];
  entity.Psrn = responseItem[2];
  return entity;
}
```
5. Создайте серверную функцию импорта контрагентов.
```
/// <summary>
/// Импорт контрагентов.
/// </summary>
/// <param name="items">Структурированный набор данных.</param>
/// <returns>Список структурированных логов.</returns>
[Remote]
public virtual void ImportEntities(List<DirRX.Solution.Structures.ImportRuleCounterparty.CounterpartyStructure > items, List<DirRX.Solution.Structures.Module.LogStruct> logs)
{
  foreach (var item in items)
  {
    var counterparty = Sungero.Parties.Counterparties.Create();
    counterparty.Name = item.Name;
    counterparty.TIN = item.Tin;
    counterparty.PSRN = item.Psrn;
    counterparty.Save();
  }
}
```
6. Переопределите функцию инициализации модуля Integration. При инициализации необходимо создать правила по умолчанию и выдать на них права всем пользователям.
```
/// <summary>
/// Выдача прав на справочники, соответствующие правилам.
/// </summary>
public override void GrantRightsOnDatabooks()
{
  base.GrantRightsOnDatabooks();
  var allUsers = Roles.AllUsers;
  if (allUsers != null)
  {
    DirRX.Solution.ImportRuleCounterparties.AccessRights.Grant(allUsers,DefaultAccessRightsTypes.Read);
    DirRX.Solution.ImportRuleCounterparties.AccessRights.Save();
  }
}

/// <summary>
/// Создание правил (записей справочников) по умолчанию.
/// </summary>
public override void CreateDefaultDatabooksItems()
{
  base.CreateDefaultDatabooksItems();
  var importRuleCounterpartyItem =
  DirRX.Solution.ImportRuleCounterparties.GetAll().FirstOrDefault();
  if (importRuleCounterpartyItem == null)
  {
    importRuleCounterpartyItem = DirRX.Solution.ImportRuleCounterparties.Create();
    importRuleCounterpartyItem.Name = DirRX.Solution.ImportRuleCounterparties.Info.LocalizedName;
    importRuleCounterpartyItem.Note = DirRX.Solution.ImportRuleCounterparties.Resources.RuleNote;
    importRuleCounterpartyItem.Save();
  }
}
```

### Изменение правила импорта
Общий порядок изменения стандартных правил ImportRuleEmployees, ImportRuleDepartments, ImportRuleJobtitles:
1. В своем решении перекройте необходимое правило.
2. Реализуйте новую структуру для хранения данных, полученных из внешней системы, если набор получаемых полей отличается от существующей структуры.
3. В зависимости от поставленной задачи создайте новые функции или переопределите существующие функции правил:
   * SaveData() – сохранение полученных данных в Directum RX;
   * ParseResponseItem() – обработка строки матрицы с ответом от внешней системы;
   * ImportEntities() – импорт сущности.
4. Переопределите соответствующую функцию импорта данных в сущность Directum RX. Если импорт будет выполняться в новую сущность, создайте новую функцию импорта.

#### Пример обработки нового свойства при импорте подразделений 
1. Создайте структуру, например DepartmentCustom, для хранения получаемых данных.
```
/// <summary>
/// Структура для работы с подразделениями.
/// </summary
partial class CustomDepartment
{
  public string Name {get; set;}
  public string Id {get; set;}
  public string BusinessUnitCode {get; set;}
  public string HeadOfficeId {get; set;}
  public string ManagerId {get; set;}
  public Sungero.Core.Enumeration Status {get; set;}
  public string NewCustomParameter {get; set;}
}
```
2. Переопределите серверную функцию SaveData():
```
/// <summary>
/// Сохранение полученных данных в Directum RX.
/// </summary>
/// <param name="response">Матрица с ответом от внешней системы.</param>
/// <param name="logs">Структурированный лог.</param>
public override void SaveData(List<string[]> response, List<DirRX.Integration.Structures.Module.LogStruct> logs)
{
  var departments = new List<DirRX.Integration.Structures.ImportRuleDepartment.DepartmentCustom>();
  foreach (var responseItem in response)
    entities.Add(ParseResponseItemCustom(responseItem));
  if (departments.Any())
  {
    ImportDepartmentsCustom(departments, logs);
  }
}
```
3. Создайте серверную функцию ParseResponseItemCustom(), которая обрабатывает строки матрицы с ответом от внешней системы:
```
/// <summary>
/// Обработка строки матрицы с ответом от внешней системы.
/// </summary>
/// <param name="responseItem">Строка матрицы с ответом от внешней системы.</param>
/// <returns>Свойства сущности в структурированном виде.</returns>
public virtual DirRX.Integration.Structures.ImportRuleDepartment.DepartmentCustom ParseResponseItemCustom(string[] responseItem)
{
  // Обработка строки матрицы.
  ...
}
```
4. Создайте серверную функцию ImportDepartmentsCustom(), которая импортирует подразделения в Directum RX:
```
/// <summary>
/// Импорт новой сущности в Directum RX.
/// </summary>
/// <param name="items">Структурированный набор данных.</param>
/// <returns>Список структурированных логов.</returns>
[Remote]
public virtual void ImportDepartmentsCustom(List<DirRX.Integration.Structures.ImportRuleDepartment.DepartmentCustom> items, List<DirRX.Integration.Structures.Module.LogStruct> logs)
{
  // Импорт подразделения.
  ...
}
```

## Порядок установки
Для работы требуется установленный Directum RX версии 4.12 и выше. 

## Установка для ознакомления
1. Склонировать репозиторий с rx-template-integration-rules в папку.
2. Указать в _ConfigSettings.xml DDS:
```xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" />
  <repository folderName="<Папка из п.1>" solutionType="Work"
     url="https://github.com/DirectumCompany/rx-template-integration-rules" />
</block>
``` 

## Установка для использования на проекте
Возможные варианты

**A. Fork репозитория**
1. Сделать fork репозитория rx-template-integration-rules для своей учетной записи.
2. Склонировать созданный в п. 1 репозиторий в папку.
3. Указать в _ConfigSettings.xml DDS:
```xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" />
  <repository folderName="<Папка из п.2>" solutionType="Work"
     url="<Адрес репозитория gitHub>" />
</block>
```

**B. Подключение на базовый слой.**
Вариант не рекомендуется, так как при выходе версии шаблона разработки не гарантируется обратная совместимость.

1. Склонировать репозиторий rx-template-integration-rules в папку.
2. Указать в _ConfigSettings.xml DDS:
```xml
<block name="REPOSITORIES">
  <repository folderName="Base" solutionType="Base" url="" />
  <repository folderName="<Папка из п.1>" solutionType="Base"
     url="<Адрес репозитория gitHub>" />
  <repository folderName="<Папка для рабочего слоя>" solutionType="Work"
     url="<Адрес репозитория для рабочего слоя>" />
</block>

```

**C. Копирование репозитория в систему контроля версий.**
Рекомендуемый вариант для проектов внедрения.
1. В системе контроля версий с поддержкой git создать новый репозиторий.
2. Склонировать репозиторий <Название репозитория> в папку с ключом `--mirror`.
3. Перейти в папку из п. 2.
4. Импортировать клонированный репозиторий в систему контроля версий командой:
`git push –mirror <Адрес репозитория из п. 1>`

