$srcSql = 'd:\CoreUI\1.MVC FINAL\DiasenseSoftware\DataAccess\SqlTempAccess.cs'
$destSql = 'd:\CoreUI\MVC_API_UI\API\DataAccess\SqlTempAccess.cs'
$content = Get-Content $srcSql -Raw
$content = $content -replace 'namespace DiasenseSoftware\.DataAccess', 'namespace Dia_CoreAPI.DataAccess'
$content = $content -replace 'internal class SqlTempAccess', 'public class SqlTempAccess'
$content = $content -replace 'using System\.Configuration;\r?\n', ''
$content = $content -replace 'using System\.Web;\r?\n', ''
$content = $content -replace 'using System\.Data\.SqlClient;', 'using Microsoft.Data.SqlClient;'
$content = $content -replace '(?s)public string GetConnectionString\(string name\)\s*\{.*?\}\s*\r?\n', ''
$content = $content -replace 'string connectionString = GetConnectionString\("localconnection"\);', 'string connectionString = _connectionString;'
$ctor = @"

        private readonly string _connectionString;

        public SqlTempAccess(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

"@
$content = $content -replace 'private IDbConnection _connection;', ($ctor + '        private IDbConnection _connection;')
Set-Content -Path $destSql -Value $content -Encoding UTF8
Write-Host "SqlTempAccess: $((Get-Item $destSql).Length) bytes"
