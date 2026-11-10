import { useParams } from 'react-router'
import { ServiceDetail } from '../features/services/ServiceDetail'

export function ServicePage() {
  const { id } = useParams()
  return <ServiceDetail key={id} id={Number(id)} />
}
