## Развертывание серверной части Recommendations.API
Процесс разворачивания можно разделить на три части: настройка БД, импорт данных в БД, настройка веб сервера, хостинг API.
##### Подготовка
1. Загружаем репозиторий
`git clone [https://github.com/ar4ebald/Recommendations.API.git`
2. Загружаем [папку с данными модели](https://drive.google.com/open?id=16BM9hYcCfU8HfDKhaMmTLjJ84ai4GstD)
3. Загружаем [папку со скриптом рекомендации](https://drive.google.com/open?id=1gtv1mE_gOq2kRcSl3aCesQ2IXrvymbA8)
4. Утанавливаем последний [.NET Core SDK](https://dotnet.microsoft.com/download)
5. Устанавливаем [Python](https://www.python.org/downloads/windows/) **обязательно x64**
##### Настройка БД
Для работы приложения потребуется PostgreSQL не менее десятой версии.
1. Создайте  пользователя recommendations_api.
2. Создайте необходимые таблицы и процедуры выполнив `src\Recommendations.API\CreateSchema.sql`.
3. Для добавления тестового оператора системы выполните скрипт:
```sql
insert into rdb.operator (login, password, roles)
values ('admin', 'password', '{admin,business}'::text[]);
```
4. В дальнейшем Вам понадобиться строка подключения к базе:
`Host=%HOST%;Port=%PORT%;Username=recommendations_api;Password=%PWD%;Database=recommendations`
Где `%HOST%`, `%PORT%` - имя и порт сервера, `%PWD%` - пароль пользователя указанный при его создании
5. Загрузите денные модели в базу:
В папке `src\Recommendations.API\Recommendations.DB.ImportUtil`  выполните `dotnet run`. Первым параметром укажите строку подключения, вторым параметром путь к папке с данными модели
#### Создание дистрибутива
Выполните `dotnet publish -c release` в папке `src\Recommendations.API\Recommendations.API`
Дистрибутив приложения будет находиться в папке `src\Recommendations.API\Recommendations.API\bin\release\netcoreapp2.1\publish`
Далее необходимо изменить файл настроек в дистрибутиве (`appsettings.json`) следующим образом:
* В качестве `ConnectionString` указать полученную раннее строку подключения.
* В `PythonPath` указать полный путь к уставленному интерпретатору питона. (Например `C:\Windows\py.exe`)
* В `PredictionScriptPath` указать путь к файлу `predict.py` в папке пункта №3 подготовки.

Дистрибутив готов для публикации!
#### Размещение дистрибутива
