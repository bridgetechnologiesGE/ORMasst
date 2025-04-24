# ORMasst
Simple Library to provide fast map between queries and structures

# Usage
Simply create your class that reflect query results and map class that provide bindings between properties and fields, then create DbConnection (or DbCommand) and invoke extension to retrieve data.

If you prefere you can also create the class and provide a dynamic mapper to extension method, avoid implementation of data and map class.
