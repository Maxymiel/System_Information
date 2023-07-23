# Инструкция
1. Скопируйте папку HTML в сетевое или доступное по FTP протоколу месторасположение<br>
2. Скопируйте скомпилированные файлы System_Information в сетевой расположение, откуда компьютеры в сети смогут их запускать<br>
## Сбор данных
<p>Настройте запуск System_Information.exe на каждом компьютере (например через <a href="https://winitpro.ru/index.php/2022/05/11/zadanie-planirovshhika-gpo/">планировщик заданий и групповые политики</a>)</p>
<p>В качестве параметра запуска необходимо указать пути до папки InputFiles, например:</p>

```
\\server\System_Information.exe \\server\HTML\InputFiles
```
<p>Также можно указать FTP ссылку для загрузки в формате ftp://user:password@server, например:</p>

```
\\server\System_Information.exe ftp://tester:12345@server/InputFiles
```
## Конвертация данных
<p>Для преобразования данных по компьютерам в html файлы необходимо запустить ConverterHTML.exe и выбрать папку HTML. Также возможно передать путь до папки в качестве параметра, например:</p>

```
ConverterHTML.exe \\server\HTML
```
