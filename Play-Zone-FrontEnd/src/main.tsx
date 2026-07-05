import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import App from './App'
import RoomMenu from './pages/RoomMenu'
import './styles.css'

const path = window.location.pathname;
const Root = path === '/room-menu' || path.startsWith('/room-menu') ? RoomMenu : App;

createRoot(document.getElementById('app')!).render(
  <StrictMode>
    <Root />
  </StrictMode>,
)
