import { useState } from 'react';
import type { FormEvent } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Card, Form, Button, Alert } from 'react-bootstrap';
import { useAuth } from '../hooks/useAuth';

export default function SignupPage() {
  const { signup } = useAuth();
  const navigate = useNavigate();
  const [userName, setUserName] = useState('');
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleSubmit = async (e: FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      await signup(userName, email, password);
      navigate('/files');
    } catch (err: unknown) {
      if (err && typeof err === 'object' && 'response' in err) {
        const axiosErr = err as { response?: { status?: number } };
        if (axiosErr.response?.status === 409) {
          setError('Username already exists. Please choose another.');
        } else {
          setError('Signup failed. Please try again.');
        }
      } else {
        setError('Signup failed. Please try again.');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="row justify-content-center mt-5">
      <div className="col-md-5">
        <Card>
          <Card.Body>
            <Card.Title className="mb-3">Sign Up</Card.Title>
            {error && <Alert variant="danger">{error}</Alert>}
            <Form onSubmit={handleSubmit}>
              <Form.Group className="mb-3" controlId="signupUser">
                <Form.Label>Username</Form.Label>
                <Form.Control
                  type="text"
                  value={userName}
                  onChange={(e) => setUserName(e.target.value)}
                  required
                />
              </Form.Group>
              <Form.Group className="mb-3" controlId="signupEmail">
                <Form.Label>Email</Form.Label>
                <Form.Control
                  type="email"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  required
                />
              </Form.Group>
              <Form.Group className="mb-3" controlId="signupPassword">
                <Form.Label>Password</Form.Label>
                <Form.Control
                  type="password"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  required
                />
              </Form.Group>
              <Button type="submit" variant="primary" disabled={loading} className="w-100">
                {loading ? 'Creating account...' : 'Sign Up'}
              </Button>
            </Form>
            <div className="mt-3 text-center">
              Already have an account? <Link to="/login">Log in</Link>
            </div>
          </Card.Body>
        </Card>
      </div>
    </div>
  );
}
