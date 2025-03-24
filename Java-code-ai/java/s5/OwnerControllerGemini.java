package java.s5;

import org.springframework.data.domain.Page;
import org.springframework.data.domain.PageRequest;
import org.springframework.data.domain.Pageable;
import org.springframework.stereotype.Controller;
import org.springframework.ui.Model;
import org.springframework.validation.BindingResult;
import org.springframework.web.bind.WebDataBinder;
import org.springframework.web.bind.annotation.*;
import org.springframework.web.servlet.ModelAndView;

import javax.validation.Valid;
import java.util.List;
import java.util.Map;

/**
 * Controller for managing Owner entities.
 */
@Controller
@RequestMapping("/owners")
class OwnerControllerGemini {

    private static final String VIEW_OWNER_FORM = "owners/createOrUpdateOwnerForm";
    private static final String VIEW_FIND_OWNERS = "owners/findOwners";
    private static final String VIEW_OWNERS_LIST = "owners/ownersList";
    private static final String VIEW_OWNER_DETAILS = "owners/ownerDetails";

    private final OwnerRepository ownerRepository;
    private final VisitRepository visitRepository;

    public OwnerControllerGemini(OwnerRepository ownerRepository, VisitRepository visitRepository) {
        this.ownerRepository = ownerRepository;
        this.visitRepository = visitRepository;
    }

    @InitBinder
    public void setAllowedFields(WebDataBinder dataBinder) {
        dataBinder.setDisallowedFields("id");
    }

    @GetMapping("/new")
    public String initCreationForm(Map<String, Object> model) {
        model.put("owner", new Owner());
        return VIEW_OWNER_FORM;
    }

    @PostMapping("/new")
    public String processCreationForm(@Valid Owner owner, BindingResult result) {
        if (result.hasErrors()) {
            return VIEW_OWNER_FORM;
        }
        ownerRepository.save(owner);
        return "redirect:/owners/" + owner.getId();
    }

    @GetMapping("/find")
    public String initFindForm(Map<String, Object> model) {
        model.put("owner", new Owner());
        return VIEW_FIND_OWNERS;
    }

    @GetMapping
    public String processFindForm(@RequestParam(defaultValue = "1") int page, Owner owner, BindingResult result, Model model) {
        if (owner.getLastName() == null) {
            owner.setLastName("");
        }

        String lastName = owner.getLastName();
        Page<Owner> ownerPage = findPaginatedOwnersByLastName(page, lastName);

        if (ownerPage.isEmpty()) {
            result.rejectValue("lastName", "notFound", "not found");
            return VIEW_FIND_OWNERS;
        }

        if (ownerPage.getTotalElements() == 1) {
            return "redirect:/owners/" + ownerPage.iterator().next().getId();
        }

        return populatePaginationModel(page, model, ownerPage);
    }

    private Page<Owner> findPaginatedOwnersByLastName(int page, String lastName) {
        int pageSize = 5;
        Pageable pageable = PageRequest.of(page - 1, pageSize);
        return ownerRepository.findByLastName(lastName, pageable);
    }

    private String populatePaginationModel(int page, Model model, Page<Owner> ownerPage) {
        model.addAttribute("listOwners", ownerPage.getContent());
        model.addAttribute("currentPage", page);
        model.addAttribute("totalPages", ownerPage.getTotalPages());
        model.addAttribute("totalItems", ownerPage.getTotalElements());
        return VIEW_OWNERS_LIST;
    }

    @GetMapping("/{ownerId}/edit")
    public String initUpdateOwnerForm(@PathVariable("ownerId") int ownerId, Model model) {
        model.addAttribute(ownerRepository.findById(ownerId));
        return VIEW_OWNER_FORM;
    }

    @PostMapping("/{ownerId}/edit")
    public String processUpdateOwnerForm(@Valid Owner owner, BindingResult result, @PathVariable("ownerId") int ownerId) {
        if (result.hasErrors()) {
            return VIEW_OWNER_FORM;
        }
        owner.setId(ownerId);
        ownerRepository.save(owner);
        return "redirect:/owners/" + ownerId;
    }

    @GetMapping("/{ownerId}")
    public ModelAndView showOwner(@PathVariable("ownerId") int ownerId) {
        Owner owner = ownerRepository.findById(ownerId);
        owner.getPets().forEach(pet -> pet.setVisitsInternal(visitRepository.findByPetId(pet.getId())));
        return new ModelAndView(VIEW_OWNER_DETAILS, "owner", owner);
    }
}