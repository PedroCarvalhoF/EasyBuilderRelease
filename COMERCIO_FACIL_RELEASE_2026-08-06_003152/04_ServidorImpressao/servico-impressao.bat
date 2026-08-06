@echo off
echo ================================
echo GERENCIANDO SERVICO IMPRESSAO
echo ================================

echo.
echo Parando servico (se existir)...
sc stop EasyManagerServidorImpressao >nul 2>&1

echo Removendo servico (se existir)...
sc delete EasyManagerServidorImpressao >nul 2>&1

echo Aguardando limpeza do servico...
timeout /t 2 >nul

echo Criando servico...
sc create EasyManagerServidorImpressao binPath= "C:\EasyManagerServidorImpressao\EasyManagerServidorImpressoras.exe" start= auto

echo Iniciando servico...
sc start EasyManagerServidorImpressao

echo.
echo Verificando status...
sc query EasyManagerServidorImpressao

echo.
echo ================================
echo FINALIZADO
echo ================================
pause