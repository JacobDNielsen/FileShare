import { Outlet, Link, useNavigate } from 'react-router-dom';
import { Navbar, Nav, Container, Button } from 'react-bootstrap';
import { useAuth } from '../hooks/useAuth';

export default function Layout() {
  const { isLoggedIn, username, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    <>
      <Navbar bg="dark" variant="dark" expand="sm" className="mb-3">
        <Container>
          <Navbar.Brand as={Link} to="/">
            FileShare
          </Navbar.Brand>
          <Navbar.Toggle />
          <Navbar.Collapse>
            {isLoggedIn && (
              <Nav className="me-auto">
                <Nav.Link as={Link} to="/files">
                  Files
                </Nav.Link>
              </Nav>
            )}
            <Nav className="ms-auto align-items-center">
              {isLoggedIn ? (
                <>
                  <Navbar.Text className="me-2">
                    Signed in as: <strong>{username}</strong>
                  </Navbar.Text>
                  <Button variant="outline-light" size="sm" onClick={handleLogout}>
                    Logout
                  </Button>
                </>
              ) : (
                <>
                  <Nav.Link as={Link} to="/login">
                    Login
                  </Nav.Link>
                  <Nav.Link as={Link} to="/signup">
                    Sign Up
                  </Nav.Link>
                </>
              )}
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>
      <Container>
        <Outlet />
      </Container>
    </>
  );
}
