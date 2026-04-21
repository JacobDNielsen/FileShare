import { Pagination as BsPagination } from 'react-bootstrap';

interface Props {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
}

export default function Pagination({ page, totalPages, onPageChange }: Props) {
  if (totalPages <= 1) return null;

  const items = [];
  for (let i = 1; i <= totalPages; i++) {
    items.push(
      <BsPagination.Item key={i} active={i === page} onClick={() => onPageChange(i)}>
        {i}
      </BsPagination.Item>,
    );
  }

  return (
    <BsPagination className="justify-content-center mt-3">
      <BsPagination.Prev disabled={page <= 1} onClick={() => onPageChange(page - 1)} />
      {items}
      <BsPagination.Next disabled={page >= totalPages} onClick={() => onPageChange(page + 1)} />
    </BsPagination>
  );
}
