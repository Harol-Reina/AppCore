namespace AppCore.Infrastructure.Data.DAOs.Enum;

public enum S3ObjectStatus {
    /// <summary>Contenido ha sido guardado</summary>
    Saved,

    /// <summary>El contenido ya existe y noes igual, por lo cual ha sido actualizado</summary>
    Updated,

    /// <summary>El contenido ya existe y es igual, por lo cual no es actualizado</summary>
    Equal,

    /// <summary>El contenido es null, por lo cual el archivo se elimino</summary>
    Delete
}
