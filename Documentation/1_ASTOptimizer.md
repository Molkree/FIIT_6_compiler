## Интеграция оптимизаций абстрактного синтаксического дерева между собой

### Постановка задачи
Необходимо скомбинировать созданные ранее оптимизации абстрактного синтаксического дерева так, чтобы они могли выполняться все вместе, друг за другом.

### Команда
Д. Володин, Н. Моздоров

### Зависимые и предшествующие задачи
Предшествующие: 
- все оптимизации абстрактного синтаксического дерева

### Теоретическая часть
Необходимо организовать выполнение оптимизаций абстрактного синтаксического дерева до тех пор, пока каждая из созданных оптимизаций не перестанет изменять дерево.

### Практическая часть
Для данной задачи был создан статический класс `ASTOptimizer`, содержащий единственный публичный метод `Optimize`, который получает на вход `Parser` - наше представление AST,
а также, опционально, список оптимизаций для применения. По умолчанию применяются все оптимизации.
Приватное свойство `ASTOptimizations` хранит список экземпляров всех оптимизаций по AST `List<ChangeVisitor>`.
Оптимизация выполняется следующим образом: выполняется первая оптимизация из списка оптимизаций, если не было произведено никаких изменений, то выполняется вторая оптимизация из
списка, если AST было изменено в ходе оптимизации, то снова выполняется первая оптимизация. Данный процесс повторяется, пока не будет применён весь список оптимизаций и каждая
из этих оптимизаций не перестанет менять абстрактное синтаксическое дерево.

```csharp
public static void Optimize(Parser parser, IReadOnlyList<ChangeVisitor> Optimizations = null)
{
    Optimizations ??= ASTOptimizations;
    var optInd = 0;
    do
    {
        parser.root.Visit(Optimizations[optInd]);
        if (Optimizations[optInd].Changed)
        {
            optInd = 0;
        }
        else
        {
            ++optInd;
        }
    } while (optInd < Optimizations.Count);
}
```

### Место в общем проекте (Интеграция)
Данная оптимизация объединяет созданные ранее оптимизации абстрактного синтаксического дерева.

### Тесты
Тестирование устроено следующим образом: сначала по исходному коду программы генерируется абстрактное синтаксическое дерево, затем задаётся список оптимизаций для проверки,
после этого вызывается метод `Optimize` класса `ThreeAddressCodeOptimizer`, полученное AST переводится обратно в массив строк-инструкций, а этот массив сравнивается с
ожидаемым результатом. Ниже приведён один из тестов.

```csharp
[Test]
public void SubItselfSumZero()
{
    var AST = BuildAST(@"
var a;
a = a - ((a - a) + a);
");
    var optimizations = new List<ChangeVisitor>
    {
        new OptExprSumZero(),
        new OptExprSubEqualVar()
    };

    var result = ApplyOptimizations(AST, optimizations);
    var expected = new[]
    {
        "var a;",
        "a = 0;"
    };

    CollectionAssert.AreEqual(expected, result);
}
```
