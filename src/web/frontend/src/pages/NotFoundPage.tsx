import { Link } from 'react-router-dom';
import { Alert } from 'react-bootstrap';

export default function NotFoundPage() {
  return (
    <div className="mt-5 text-center">
      <Alert variant="warning">
        <Alert.Heading>404 - Page Not Found</Alert.Heading>
        <p>The page you are looking for does not exist.</p>
        <Link to="/files">Go to Files</Link>
      </Alert>
    </div>
  );
}
