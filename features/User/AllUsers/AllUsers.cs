using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Features.User.Entities;
using Features.User.DTOs;

public class UserList : List<User>, IEnumerable<User>
{
    public UserList(List<User> users) : base(users) { }

    public new IEnumerator<User> GetEnumerator()
    {
        foreach (var user in this)
        {
            yield return user;
        }
    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }



}
