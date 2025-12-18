import httpx
import json

class LoggingAsyncHttpClient(httpx.AsyncClient):
    async def send(self, request, *args, **kwargs):
        # --- REQUEST ---
        print("=== INTERCEPTED ASYNC HTTP REQUEST ===")
        print(request.method, request.url)
        # Be careful: this includes auth headers if you print everything.
        print("Headers:", dict(request.headers))

        # request.content is bytes or None for JSON requests
        if request.content:
            try:
                body = request.content.decode("utf-8")
            except Exception:
                body = str(request.content)
            print("Body:", body)

        # Perform actual request
        response = await super().send(request, *args, **kwargs)

        # --- RESPONSE ---
        print("=== INTERCEPTED ASYNC HTTP RESPONSE ===")
        print("Status:", response.status_code)
        
        try:
            print("Body:", response.text)
        except Exception:
            print("Body: <unprintable>")

        return response