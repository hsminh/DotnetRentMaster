namespace RentMaster.Core.Utils;

public static class MarkPassword
{
    public static void MaskPasswordIfExist(object entityOrList)
    {
        if (entityOrList is IEnumerable<object> list)
        {
            foreach (var item in list)
            {
                MaskSingleEntity(item);
            }
        }
        else
        {
            MaskSingleEntity(entityOrList);
        }
    }

    private static void MaskSingleEntity(object entity)
    {
        var passwordProp = entity.GetType().GetProperty("Password");
        if (passwordProp != null)
        {
            var value = passwordProp.GetValue(entity);
            if (value != null && !string.IsNullOrEmpty(value.ToString()))
            {
                passwordProp.SetValue(entity, "*************");
            }
        }
    }
}