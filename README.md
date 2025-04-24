# ORMasst
Simple Library to provide fast map between queries and net core classes.

# Usage
Simply create your class that reflect query results and then create map class that provide bindings between class properties (or fields) and query fields, then create DbConnection (or DbCommand) and invoke extension using mapper to retrieve data.

If you prefere you can also create the class and provide a dynamic mapper to extension method, avoid implementation of data and map class.
