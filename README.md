# maggsoft

![maggsoft](https://user-images.githubusercontent.com/3499783/235142530-b76cbf78-71ba-40fa-acea-e154c518f894.jpg)

#.net 5

aynı sunucudan apiye istek atılabilmesi için eklenmelidir.


    if (!app.Environment.IsDevelopment())
    {
        app.Use(async (context, next) =>
        {
            if (!context.Request.IsLocal())
            {
                // Forbidden http status code
                context.Response.StatusCode = 403;
                return;
            }

            await next.Invoke();
        });
    }
