call heroku login
call heroku container:login
call heroku container:push web
call heroku container:release web
call heroku open
call heroku logs --tail