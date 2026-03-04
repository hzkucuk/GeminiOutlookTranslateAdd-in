@echo off
:: Gemini Translate - Sertifika Kurulum (MSI Custom Action)
:: Bu dosya MSI kurulumu sirasinda otomatik calistirilir.

certutil -addstore "Root" "%~dp0GeminiTranslate-CodeSigning.cer" >nul 2>&1
certutil -addstore "TrustedPublisher" "%~dp0GeminiTranslate-CodeSigning.cer" >nul 2>&1
exit /b 0
