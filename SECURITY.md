# Безопасность API

## Что реализовано

1. **JWT-аутентификация**
   - Вход: `POST /api/auth/login` с телом `{ "userName": "admin", "password": "admin" }`.
   - Ответ: `{ "token": "...", "expiresAt": "...", "userName": "admin", "role": "Admin" }`.
   - Все запросы к `/api/Documents` требуют заголовок: `Authorization: Bearer {token}`.

2. **Роли**
   - **Admin** — полный доступ: GET, POST, PUT, DELETE.
   - **User** — чтение и создание: GET (список, по Id), POST (создать документ). PUT и DELETE возвращают 403.

3. **Пароли**
   - Хэширование BCrypt (не храним пароли в открытом виде).
   - Демо-пользователи: `admin/admin`, `user/user`. В продакшене — только из БД/секретов.

4. **Конфигурация**
   - Секрет JWT (`Jwt:Key`) в продакшене задаётся только через переменную окружения `Jwt__Key` (минимум 32 символа для HS256).
   - В Development в `appsettings.Development.json` указан демо-ключ для локального запуска.

## Как проверить

1. **Без токена** — запрос к `GET /api/Documents` возвращает **401 Unauthorized**.
2. **С токеном User** — GET и POST работают; PUT и DELETE возвращают **403 Forbidden**.
3. **С токеном Admin** — все операции (GET, POST, PUT, DELETE) доступны.
4. **Swagger** — кнопка «Authorize», вставить токен (без слова Bearer — Swagger сам добавит), после этого все запросы из UI идут с токеном.

## Пример запроса (curl)

```bash
# 1. Получить токен
curl -X POST https://localhost:5260/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"userName":"admin","password":"admin"}'

# 2. Запрос с токеном (подставить YOUR_TOKEN из ответа)
curl -X GET https://localhost:5260/api/Documents \
  -H "Authorization: Bearer YOUR_TOKEN"
```

## Ключевые решения

- **Аутентификация:** JWT в заголовке `Authorization: Bearer`, проверка подписи и срока действия на каждом запросе.
- **Авторизация:** роли в claims токена, атрибуты `[Authorize]` и `[Authorize(Roles = "Admin")]` на контроллере/действиях.
- **Безопасность паролей:** BCrypt, в продакшене пользователи и хэши — в БД.
- **Секреты:** JWT Key и строка подключения в продакшене только через переменные окружения, не в репозитории.
