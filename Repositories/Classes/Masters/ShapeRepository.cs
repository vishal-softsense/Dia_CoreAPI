using Dia_CoreAPI.DataAccess;
using Dia_CoreAPI.Models;
using Dia_CoreAPI.Repositories.Interfaces;
using Microsoft.Data.SqlClient;
using System.Data;

namespace Dia_CoreAPI.Repositories.Implementations
{
    public class ShapeRepository : IShapeRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;
      
        public ShapeRepository(
            IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<List<ShapeMaster>>
            GetShapeMasterListAsync(string condition)
        {
            List<ShapeMaster> data =
                new List<ShapeMaster>();

            using var connection =
                (SqlConnection)_connectionFactory
                    .CreateConnection();

            string query =
                @"EXEC GetALLMasterList
                    @Type='Shape',
                    @Where=@Where";

            using var command =
                new SqlCommand(query, connection);

            command.Parameters.AddWithValue(
                "@Where",
                condition ?? "");

            await connection.OpenAsync();

            using var reader =
                await command.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                data.Add(new ShapeMaster
                {
                    ID =
                        Convert.ToInt32(reader["ID"]),

                    ShapeID =
                        reader["ShapeID"]?.ToString() ?? "",

                    ShapeName =
                        reader["ShapeName"]?.ToString() ?? "",

                    ShapePriority =
                        reader["ShapePriority"] == DBNull.Value
                        ? 0
                        : Convert.ToSingle(
                            reader["ShapePriority"]),

                    ShapeGroupName =
                        reader["ShapeGroupName"]?.ToString() ?? "",

                    ShapeGroupPriority =
                        reader["ShapeGroupPriority"] == DBNull.Value
                        ? 0
                        : Convert.ToSingle(
                            reader["ShapeGroupPriority"]),

                    PricingShapeID =
                        reader["PricingShapeID"]?.ToString() ?? "",

                    CompanyID =
                        reader["CompanyID"] == DBNull.Value
                        ? 0
                        : Convert.ToInt32(
                            reader["CompanyID"])
                });
            }

            return data;
        }

        public async Task<bool>
            SaveShapeMasterDataAsync(
                int id,
                string shapeId,
                string shapeName,
                float shapePriority,
                string shapeGroupName,
                float shapeGroupPriority,
                string pricingShapeId,
                string createdBy,
                string modifyBy,
                int companyId,
                string actionType)
        {
            using var connection =
                (SqlConnection)_connectionFactory
                    .CreateConnection();

            using var command =
                new SqlCommand(
                    "SP_ShapeMaster",
                    connection);

            command.CommandType =
                CommandType.StoredProcedure;

            command.Parameters.AddWithValue(
                "@ID",
                id);

            command.Parameters.AddWithValue(
                "@ShapeID",
                shapeId ?? "");

            command.Parameters.AddWithValue(
                "@ShapeName",
                shapeName ?? "");

            command.Parameters.AddWithValue(
                "@ShapePriority",
                shapePriority);

            command.Parameters.AddWithValue(
                "@ShapeGroupName",
                shapeGroupName ?? "");

            command.Parameters.AddWithValue(
                "@ShapeGroupPriority",
                shapeGroupPriority);

            command.Parameters.AddWithValue(
                "@PricingShapeID",
                pricingShapeId ?? "");

            command.Parameters.AddWithValue(
                "@CreatedBy",
                createdBy ?? "");

            command.Parameters.AddWithValue(
                "@ModifyBy",
                modifyBy ?? "");

            command.Parameters.AddWithValue(
                "@CompanyID",
                companyId);

            command.Parameters.AddWithValue(
                "@ActionType",
                actionType);

            await connection.OpenAsync();

            int result =
                await command.ExecuteNonQueryAsync();

            return result > 0;
        }
    }
}