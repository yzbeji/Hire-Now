import { useParams } from 'react-router'
import { SellerProfileView } from '../features/services/SellerProfileView'

export function SellerProfilePage() {
  const { id = '' } = useParams()
  return <SellerProfileView key={id} userId={id} />
}
