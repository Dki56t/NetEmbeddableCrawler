Описание:
1. Все параметры задаются в конфигурационном файле, интерфейс их ввода отсутствует. Переменная ${TempPath} означает использовать временную директорию текущего пользователя.
2. По запросу на обработку ссылки http://site.com/ и выгрузки в папку %TEMP%/Out будет сформирована папка %TEMP%/Out/site.com
	содержащая файл index.html. В зависимости от параметра FullTraversal в директории %TEMP%/Out могут быть сформированы и другие папки 
	по имени хостов, на которые были обнаружены ссылки в процессе обхода
3. Обход сайта http://html-agility-pack.net/ с глубиной 2 и FullTraversal=true занимал у меня около 8 минут.

Ограничения:
1. Контент, на который имются ссылки из кода js, css, тескстовых файлов не собирается краулером. Обрабатываются только HTML страницы.
2. Контент не загружается в параллельных потоках. Это можно было бы сделать, но сильно увеличило бы сложность. На практике, я бы развивал решение задачи в этом направлении,
	для тестового задания углубляться в эту сторону не стал. Сначала планировал решить задачу таким образом, но когда понял что сложность слишком растет, откатил изменения.
	Некоторые артефакты в исходном коде оставил как задел на будущее.
3. Логирования исключений не ведется, т.к. это не относится напрямую к задаче.
4. Все дерево элементов держится в памяти до построения полного дерева. Т.е. при большой глубине обхода может возникнуть недостаток памяти.
5. Тестовая страница, предлагаемая по-умолчанию принадлежит утилите для разбора Html. Я замечал, что иногда под натиском краулера они режут часть запросов, из-за чего страница
	может собраться не полностью. Если возникают какие-то проблемы, можно вызвать краулер повторно.
6. Атрибуты integrity и crossorigin удаляются из html, т.к. файлы меняются после обработки.
7. Ссылки вида http://site/some-content приводятся к виду http://site/some-content.html для исключения конфликтов с директориями
8. Если при попытке скачать контент удаленным сервером выдается исключение вида разрыва соединения (ru.linkedin.com, например) или AccessDenied
	контент не скачивается,	ссылка на него в исходном html все равно изменяется, исключение подавляется.
9. Все пути строятся относительно хостов, для большей целостности контента. Т.е. при перемещении выходной папки в другую директорию ссылки
	ведущие на другие сайты (отличные от целевого) не смогут открыться. В противном случае мы не смогли бы полноценно работать со страницами смежных сайтов.
10. Если ссылка на контент слишком длинна для помещения в файловую систему (определяю как > 200 символов), или строка параметров слишком длинна, 
	она заменяется на Guid,	т.е. структура файлов не детерминированна.

<?xml version="1.0" encoding="utf-8"?>

<configuration>
  <startup>
    <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.6.1" />
  </startup>
  <appSettings>
    <add key="Depth" value="2" />
    <add key="RootLink" value="http://html-agility-pack.net/" />
    <add key="FullTraversal" value="true" />
    <add key="DestinationFolder" value="${TempPath}\TestFileWrite" />
  </appSettings>
</configuration>

using System;
using System.Configuration;
using System.IO;
using Crawler.Logic;

namespace Crawler
{
    internal class Program
    {
        private static void Main()
        {
            var loader = new FileLoader();
            var tmpPath = Path.GetTempPath();
            tmpPath = tmpPath.Remove(tmpPath.LastIndexOf(Path.DirectorySeparatorChar));

            var cfg = new Configuration
            {
                RootLink = ConfigurationManager.AppSettings["RootLink"],
                Depth = Convert.ToInt16(ConfigurationManager.AppSettings["Depth"]),
                DestinationFolder = ConfigurationManager.AppSettings["DestinationFolder"]
                    .Replace("${TempPath}", tmpPath),
                FullTraversal = Convert.ToBoolean(ConfigurationManager.AppSettings["FullTraversal"])
            };

            var mapper = new UrlMapper(cfg);
            var builder = new ItemBuilder(cfg, mapper);
            var root = builder.Build(loader).Result;

            ItemWriter.Write(root, mapper);
        }
    }
}