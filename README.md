# Reviews
API - http://mem123.azurewebsites.net/api/generate<br>
Website - https://blazorreviews.azurewebsites.net<br>
<br>
I am not sure why extra manual steps are needed, but the Docker image DOES work.<br>
<br>
First, I had to execute this command from the command prompt:<br>
docker run -p 8080:80 blazorreviews<br>
<br>
Then, I had to specify localhost with port 8080 in the browser after starting the debugger.<br>
http://localhost:8080/<br>
<br>
The debugger launches a web browser with a different port number, which refuses to connect.<br>
