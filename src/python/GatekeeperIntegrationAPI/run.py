import uvicorn

if __name__ == '__main__':
    uvicorn.run(
        'app.main:app',
        host='0.0.0.0',
        port=8042,
        reload=True,
        forwarded_allow_ips='*',
        proxy_headers=True
    )
