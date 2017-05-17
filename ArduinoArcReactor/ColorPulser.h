struct Color
{
  Color(uint8_t r, uint8_t g, uint8_t b) : R(r), B(b), G(g) {}
  uint8_t R;
  uint8_t G;
  uint8_t B;
};

class ColorPulser
{
public:
  ColorPulser(const Color& from, const Color& to) : _from(from), _to(to), _current(from)
  {
  }
  bool Animate()
  {   
    auto changed =
      AnimateChannel(_current.R, _to.R) ||
      AnimateChannel(_current.G, _to.G) ||
      AnimateChannel(_current.B, _to.B);
    if (!changed)
    {     
      _to = _from;
      _from = _current;
    }
    return changed;
  }
  const Color& Value() const { return _current; }

private:
  static bool AnimateChannel(uint8_t& current, uint8_t to)
  {
    auto changed(false);
    if (current < to)
    {
      current++;
      changed = true;
    }
    else if (current > to)
    {
      current--;
      changed = true;
    }    
    return changed;
  }

  Color _from;
  Color _to;
  Color _current;
};

