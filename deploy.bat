heroku container:login
heroku container:push web
heroku container:release web
heroku open
heroku logs --tail