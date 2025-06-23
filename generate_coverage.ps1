# Script PowerShell para executar testes, coletar cobertura e gerar relatório HTML usando XPlat Collector (sem dependência de Coverlet.MSBuild)

# Caminhos
$repoRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
$testProject = Join-Path $repoRoot "ReservaSalas.Tests/ReservaSalas.Tests.csproj"
$reportDir = Join-Path $repoRoot "coverage/report"

# Limpa relatórios anteriores
if (Test-Path $reportDir) { Remove-Item -Recurse -Force $reportDir }
New-Item -ItemType Directory -Path $reportDir | Out-Null

# Executa testes com coleta de cobertura via XPlat
Write-Host "Executando testes com Code Coverage Collector..."
dotnet test $testProject --results-directory "$repoRoot/coverage/testresults" --collect:"XPlat Code Coverage"
if ($LASTEXITCODE -ne 0) { Write-Error "Falha nos testes."; exit $LASTEXITCODE }

# Localiza arquivo de cobertura gerado (.cobertura.xml)
Write-Host "Buscando arquivo de cobertura..."
$coverageFile = Get-ChildItem -Path "$repoRoot/coverage/testresults" -Recurse -Include "coverage.cobertura.xml" | Select-Object -First 1
if (-not $coverageFile) { Write-Error "Arquivo de cobertura não encontrado."; exit 1 }

# Gera relatório HTML
Write-Host "Gerando relatório HTML em $reportDir..."
reportgenerator -reports:"$($coverageFile.FullName)" -targetdir:"$reportDir" -reporttypes:Html
if ($LASTEXITCODE -ne 0) { Write-Error "Erro ao gerar relatório."; exit $LASTEXITCODE }

# Abrir relatório
$index = Join-Path $reportDir "index.htm"
Write-Host "Abrindo relatório: $index"
Start-Process $index

