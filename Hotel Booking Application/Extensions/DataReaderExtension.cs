using Microsoft.Data.SqlClient;

namespace Hotel_Booking_Application.Extensions
{
    public static class DataReaderExtension
    {
        public static T GetValeByColumn<T>(this SqlDataReader reader , string columnName)
        {
            // get the 0 based index of the column in the table
            // if the column does not exist, it will throw an exception
            int columnIndex = reader.GetOrdinal(columnName);

            // get the value of that cell in the column if it is not null
            if (!reader.IsDBNull(columnIndex))
            {
                // get the vakue of the cell in the column and cast it to the type T then return it
                return (T)reader.GetValue(columnIndex);
            }
            //Handling Null Values :
            //If the value in that cell is null, the method returns the default value for the type T.
            //The default value depends on what T is; for reference types, it is null,
            //and for value types, it is typically zero or a struct with all zero values.
            return default(T);
        }
    }
}
