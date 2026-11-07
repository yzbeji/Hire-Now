import { AreaNotice } from '../components/AreaNotice'
import { Button, Stack, Typography } from '@mui/material'
import { useParams } from 'react-router-dom'
import { useAuth } from '../features/auth/useAuth'
import { useCurrentUser } from '../features/auth/useCurrentUser'
import { useQuery } from '@tanstack/react-query'
import { apiGet } from '../api/client'

type Role = 'Seller' | 'Buyer' | 'Admin'
type User = { username: string; roles: Role[] }

export function ProfilePage(){
    const {id} = useParams();
    const { isReady, isAuthenticated } = useAuth()
    const {data: currentUser} = useCurrentUser()

    const { data: profileOwner } = useQuery<User>({
        queryKey: ['profile', id],
        queryFn: () => apiGet<User>(id!),
        enabled: !!id, // only runs if ID exists
    });
    

    if (!isReady) {
    return <Typography variant="body2">Checking sign-in…</Typography>
    }

    
    if(!isAuthenticated)
    {
        if(id == null)//if no given id, and not logged in - error
        {return <Typography variant="body2">No profile found...</Typography>}

        //if id given, but seller -> public
        if(profileOwner?.roles.includes("Seller"))
        {return <Typography variant="body2">Welcome to this seller's public profile!</Typography>}
    }
    else{ 
        //if no given id -> current user
        if(id == null)
        {return <Typography variant="body2">Welcome to YOUR profile!</Typography>}


        //if id given, but buyer -> MUST be logged in SELLER
        if(profileOwner?.roles.includes("Seller") && currentUser?.roles.includes("Buyer"))
            {return <Typography variant="body2">Welcome to this buyer's private profile!</Typography>}
        
    }
    
}   
