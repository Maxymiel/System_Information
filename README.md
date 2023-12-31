# Инструкция
<p>Программа для сбора данных о компьютерах в локальной сети</p>

* Скопируйте папку HTML в сетевое или доступное по FTP протоколу месторасположение<br>
* Скопируйте скомпилированные или <a href="https://github.com/Maxymiel/System_Information/releases">скачанные</a> файлы System_Information в сетевой расположение, откуда компьютеры в сети смогут их запускать

<b>:exclamation: Размещая исполняемые файлы на общих серверах, позаботьтесь о невозможность их замены третьими лицами</b>

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
## Просмотр данных
Откройте index.htm из папки HTML через любой браузер

## Пример работы
![Пример работы](https://github.com/Maxymiel/System_Information/blob/master/HTML/source/example.png)
