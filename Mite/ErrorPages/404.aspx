﻿<%@Response.StatusCode="404"%>

<!DOCTYPE html>
<html lang="ru">
<head>
    <meta charset="utf-8" />

    <link rel="apple-touch-icon" sizes="57x57" href="~/Content/icons/apple-icon-57x57.png" />
    <link rel="apple-touch-icon" sizes="60x60" href="~/Content/icons/apple-icon-60x60.png" />
    <link rel="apple-touch-icon" sizes="72x72" href="~/Content/icons/apple-icon-72x72.png" />
    <link rel="apple-touch-icon" sizes="76x76" href="~/Content/icons/apple-icon-76x76.png" />
    <link rel="apple-touch-icon" sizes="114x114" href="~/Content/icons/apple-icon-114x114.png" />
    <link rel="apple-touch-icon" sizes="120x120" href="~/Content/icons/apple-icon-120x120.png" />
    <link rel="apple-touch-icon" sizes="144x144" href="~/Content/icons/apple-icon-144x144.png" />
    <link rel="apple-touch-icon" sizes="152x152" href="~/Content/icons/apple-icon-152x152.png" />
    <link rel="apple-touch-icon" sizes="180x180" href="~/Content/icons/apple-icon-180x180.png" />
    <link rel="icon" type="image/png" sizes="192x192" href="~/Content/icons/android-icon-192x192.png" />
    <link rel="icon" type="image/png" sizes="32x32" href="~/Content/icons/favicon-32x32.png" />
    <link rel="icon" type="image/png" sizes="96x96" href="~/Content/icons/favicon-96x96.png" />
    <link rel="icon" type="image/png" sizes="16x16" href="~/Content/icons/favicon-16x16.png" />
    <meta name="msapplication-TileColor" content="#b540c6" />
    <meta name="msapplication-TileImage" content="~/Content/icons/ms-icon-144x144.png" />
    <meta name="theme-color" content="#b540c6" />
    
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>Страница не найдена</title>
    <style>
        body, .error {
            height: 100%;
        }
        body {
            background: url("/Content/images/water-back.jpg"), round;
        }
        .error {
            vertical-align: middle;
            display: flex;
            justify-content: center;
            align-items: center;
        }
    </style>
</head>
<body>
    <div class="error">
        <a href="/"><img src="/Content/images/404err.png" /></a>
    </div>
</body>
</html>